using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.LibCrossport.Settings;
using Ices.Crossport;
using Ices.Crossport.ObjectModel;
using Unity.RenderStreaming;
using UnityEngine;

public class HarryPotterDriver : CrossportDriverBase
{
#pragma warning disable 0649
    [SerializeField] private AudioStreamSender gameAudio;
    [SerializeField] private VideoStreamSender mainCamera;
    [SerializeField] private InputReceiver gameControl;
#pragma warning restore 0649
    protected override void Configure(CrossportSetting config)
    {
        config.Video(nameof(mainCamera))?.Configure(mainCamera);
        config.Audio(nameof(gameAudio))?.Configure(gameAudio);
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}