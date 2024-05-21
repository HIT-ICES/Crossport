using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Anonymous.Crossport;
using Anonymous.Crossport.Diagnostics;
using Anonymous.Crossport.ObjectModel;
using Anonymous.Crossport.Settings;
using Unity.RenderStreaming;
using Unity.RenderStreaming.Signaling;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Anonymous.Crossport.Receiver
{
    public class DrivenControlPanel : ControlPanel
    {
        [SerializeField] RemoteConfiguredReceiverCrossportDriver driver;


        protected override void OnStart()
        {
            driver.OnStart += Driver_OnStart;
            var audio = CrossportAudioSetting.Default();
            audio.key = "gameAudio";
            var video = CrossportVideoSetting.Default();
            video.key = "mainCamera";
            driver.setting = new()
                             {
                                 audio = new() { audio },
                                 video = new() { video },
                                 signaling = new()
                                             {
                                                 capacity = 0,
                                                 address = crossportSetting.address,
                                                 port = crossportSetting.port,
                                                 application = crossportSetting.application,
                                                 component = crossportSetting.component,
                                                 interval = 5.0f,
                                                 options = new(),
                                             },
                             };
            driver.RideOn();
        }

        private void Driver_OnStart(RemoteConfiguredReceiverCrossportDriver obj) { base.OnStart(); }


        protected override void OnStop()
        {
            base.OnStop();
            driver.RideOff();
        }
    }
}