using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Anonymous.Crossport;
using Anonymous.Crossport.Diagnostics;
using Anonymous.Crossport.Settings;
using Unity.RenderStreaming;
using Unity.RenderStreaming.Signaling;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Anonymous.Crossport.Receiver
{
    public partial class ControlPanel : MonoBehaviour
    {
        [SerializeField] List<string> options = new List<string>();

        private void Awake()
        {
            startButton.onClick.AddListener(OnStart);
            stopButton.onClick.AddListener(OnStop);
            var control = new CrossportUIControl();

            control.UIControl.Enable();
            RegisterEvents
            (
                control.UIControl.ToggleUI,
                c =>
                {
                    if ((c.control as ButtonControl).wasReleasedThisFrame)
                    {
                        gameObject.SetActive(!gameObject.activeSelf);
                    }
                }
            );

            //settings = SampleManager.Instance.Settings;
        }


        protected virtual void OnStart()
        {
            ConsoleManager.LogWithDebug($"Connecting to server: {crossportSetting.GetFetchAppUrl()}");


            StartCoroutine(StartAsync());
        }

        protected IEnumerator StartAsync()
        {
            yield return CrossportClientUtils.FetchAndApplyConfig(crossportSetting);
            stopButton.gameObject.SetActive(true);
            startButton.gameObject.SetActive(false);
        }

        protected virtual void OnStop() { StartCoroutine(StopAsync()); }

        protected IEnumerator StopAsync()
        {
            yield return CrossportClientUtils.SubmitStats(crossportSetting);
            startButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
        }
#pragma warning disable 0649
        [SerializeField] private Button startButton;
        [SerializeField] private Button stopButton;

        [Tooltip("Crossport Receiver Setting")]
        protected CrossportClientSetting crossportSetting = new(); // => SampleManager.Instance.CrossportSettings;
#pragma warning restore 0649
    }
}