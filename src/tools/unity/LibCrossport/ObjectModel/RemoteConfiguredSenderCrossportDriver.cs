#nullable enable
using Assets.Scripts.LibCrossport.Settings;
using BodhiDonselaar;
using Ices.Crossport;
using Ices.Crossport.ObjectModel;
using System;
using System.Collections.Generic;
using Unity.RenderStreaming;
using UnityEngine;

namespace Ices.Crossport.ObjectModel
{
    public class RemoteConfiguredSenderCrossportDriver : CrossportDriverBase
    {
        protected override void Configure(CrossportSetting config)
        {
            StartCoroutine
            (
                CrossportClientUtils.FetchAndApplyConfig(config.signaling)
                                    .Then
                                     (
                                         () =>
                                         {
                                             config.Video(nameof(mainCamera))?.Configure(mainCamera, true);

                                             config.Audio(nameof(gameAudio))?.Configure(gameAudio);
                                             if (equiCamera == null) return;
                                             var targetSolution = 1;
                                             for (;
                                                  targetSolution < Screen.width;
                                                  targetSolution <<= 1) { }

                                             //targetSolution <<= 1;
                                             equiCamera.RenderResolution = targetSolution;
                                             //mainCamera.width = Math.Min( (uint)targetSolution,3840u);
                                             //mainCamera.height = (uint)targetSolution;
                                             ConsoleManager.LogWithDebug
                                                 ($"Equi Camera configured, resolution: {targetSolution}");
                                         },
                                         StartCoroutine
                                     )
            );
        }
#pragma warning disable 0649
        [SerializeField] private AudioStreamSender gameAudio;
        [SerializeField] private VideoStreamSender mainCamera;
        [SerializeField] private EquiCam? equiCamera;
#pragma warning restore 0649
    }
}