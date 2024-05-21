using System;
using System.Collections;
using System.Collections.Generic;
using Unity.RenderStreaming;
using Unity.WebRTC;
using UnityEngine;

namespace Ices.Crossport.Diagnostics
{
    public class FrameRateRecorder : MonoBehaviour
    {
        [SerializeField] private VideoStreamReceiver? activeReceiver;

        public static double averageFps;

        public static List<double> rawFps = new();
        private static FrameRateRecorder _instance;
        private uint frameCount;
        private double durations;
        private uint remoteInitFrameCount;
        private long remoteInitTimestamp;
        private uint remoteLastFrameCount;
        private long remoteLastTimestamp;

        public static void ResetStats()
        {
            averageFps = 0;
            rawFps.Clear();
            _instance?.InstanceResetStats();
        }

        public void InstanceResetStats()
        {
            frameCount = 0;
            durations = 0;
            remoteInitTimestamp = 0;
            remoteInitFrameCount = 0;
        }

        public static Stats Export()
            => Stats.FromRawAndAvg(rawFps, averageFps);

        private void Awake() { _instance = this; }

        private void Start()
        {
            ResetStats();
            if (activeReceiver != null)
            {
                StartCoroutine(CollectStats());
                ConsoleManager.LogWithDebug("Using Receiver Framerate");
            }
            else
            {
                ConsoleManager.LogWithDebug("Using Local Framerate");
            }
        }

        void AddFps(double fps)
        {
            if (rawFps.Count <= 200) rawFps.Add(fps);
        }

        private void Update()
        {
            if (activeReceiver != null) return;
            // Local Framerate
            var dt = Time.unscaledDeltaTime;
            durations += dt;
            frameCount++;
            averageFps = frameCount / durations;
            var fps = 1.0 / dt;
            AddFps(fps);
        }

        private void OnDestroy() { }

        private IEnumerator CollectStats()
        {
            var waitSec = new WaitForSeconds(1);

            while (true)
            {
                yield return waitSec;
                if (activeReceiver?.Transceiver?.Receiver == null) continue;
                yield return StartCoroutine(UpdateStats(activeReceiver.Transceiver.Receiver));
            }
        }

        private IEnumerator UpdateStats(RTCRtpReceiver receiver)
        {
            var op = receiver.GetStats();
            yield return new WaitUntilWithTimeout(() => op.IsDone, 3f);

            if (op.IsError || !op.IsDone) yield break;

            var report = op.Value;
            if (report == null) yield break;

            // Update Report
            foreach (var stats in report.Stats.Values)
            {
                if (stats is not RTCInboundRTPStreamStats inboundStats) continue;
                if (inboundStats.kind != "video") continue;

                if (remoteInitTimestamp == 0)
                {
                    remoteInitFrameCount = inboundStats.framesDecoded;
                    remoteInitTimestamp = inboundStats.Timestamp;
                }
                else
                {
                    var totalTimeDiff = (double)(inboundStats.Timestamp - remoteInitTimestamp) / 1000000L;
                    averageFps = (inboundStats.framesDecoded - remoteInitFrameCount) / totalTimeDiff;
                    var currentTimeDiff = (double)(inboundStats.Timestamp - remoteLastTimestamp) / 1000000L;
                    var fps = (inboundStats.framesDecoded - remoteLastFrameCount) / currentTimeDiff;
                    AddFps(fps);
                }

                remoteLastFrameCount = inboundStats.framesDecoded;
                remoteLastTimestamp = inboundStats.Timestamp;
            }

            report.Dispose();
        }
    }

    internal class WaitUntilWithTimeout : CustomYieldInstruction
    {
        private readonly Func<bool> predicate;

        private readonly float timeoutTime;

        public WaitUntilWithTimeout(Func<bool> predicate, float timeout)
        {
            timeoutTime = Time.realtimeSinceStartup + timeout;
            this.predicate = predicate;
        }

        public bool IsCompleted { get; private set; }

        public override bool keepWaiting
        {
            get
            {
                IsCompleted = predicate();
                if (IsCompleted) return false;

                return !(Time.realtimeSinceStartup >= timeoutTime);
            }
        }
    }
}