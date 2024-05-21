---
sidebar_position: 0
---

# Crossport: Getting Started

Crossport is based on [Unity Render Streaming](https://docs.unity3d.com/Packages/com.unity.renderstreaming@3.1/manual/index.html), which is used to build a peer-to-peer streaming solution quickly. Render Streaming is based on WebRTC, you can find more details in it's official documentation.

Originally, Render Streaming can transport video (screen, game camera, web camera, or texture), audio (game audio or microphone) and control data (user input or game data) bidirectionally from Unity application to another Unity application or web browser. However, the official documentation only includes a public signalling server which can only serve one application at a time, and no support for container for now (2023.6).

Crossport filled the blanks. It provides a standard to make it easy to serve multiple applications with one signalling server, and enables a Unity render streaming application to run in a container with hardware acceleration.

## Prerequisites

To get started with Crossport, you need to prepare:

1. An available crossport server. This is a WebRTC signaling server implemented with C\# by us, you can just use our latest docker image and deploy it into your own edge cluster or any machine with docker.
2. A receiver client for remote rendering, which could be a web application or Unity application compatibility with Crossport or origin Render Streaming. For evaluation usage, you might use our prepared **CrossportReceiver** client.
3. A unity application which can work with the origin render streaming and browser. **Make sure you've fully understand the origin render streaming package, and read all its documentation.**

## Install RenderStreaming and LibCrossport

1. Install RenderStreaming following the [official tutorial](https://docs.unity3d.com/Packages/com.unity.renderstreaming@3.1/manual/tutorial.html).
2. Import the LibCrossport Unity Package into your application. In is composed of the following parts:

|Module|Description|Usage|
|---|---|---|
|CrossportSignaling|An `ISignalling` modified from origin `WebsocketSignalling`, which is used to connect to Crossport Server|**Automatically** created by the scripts at application start according to the crossport settings.|
|CrossportDriverBase|Base class for crossport drivers, which is intended to be implemented by yourself.|Implement the crossport driver class by yourself following the steps in the next section.|
|CrossportConfigurationManager|Utility class providing static methods to save or load crossport settings.|The `load` methods will be called **automatically** by the crossport drivers at application start. Normally, crossport settings should not be modified, so you don't need to care about `save` methods.
|CrossportSetting Records (All other files)|Data classes for crossport settings.|Created **automatically** by configuration manager. Video and audio settings provide `Configure()` methods to configure stream senders and receivers.|

## Configure RenderStreaming for Crossport

1. Turn off **Automatic Streaming** following the [official tutorial](https://docs.unity3d.com/Packages/com.unity.renderstreaming@3.1/manual/create-scene.html) ('Changing Project Settings' section).
2. Add `SignallingManager` Component to a GameObject, uncheck `Run On Awake`.
3. Add `Broadcast` Component to a GameObject, add it to the `Signaling Handler List` of `SignallingManager`.
4. Add and configure `VideoStreamSender`, `AudioStreamSender`, and `InputReceiver` on demand. Then add them to the `Streams` of `Broadcast` Component.
5. Setting up Input Actions following the [official tutorial](https://docs.unity3d.com/Packages/com.unity.renderstreaming@3.1/manual/control-camera.html), remember to check settings of Input System.

## Implement `CrossportDriver`

You should create a new script, containing a class inheriting `CrossportDriverBase`. The script should be attached to some GameObject as a Component. It contains the following members:

|Name|Type|Description|
|---|---|---|
|Stream senders and receivers|**Required** SerializeField of current class|Render streaming video/audio senders and receivers, which should be attached to components in the unity Editor UI|
|Configure(CrossportSetting)|**Required** protected virtual method|Called after crossport settings are loaded. It's normally used to configure stream senders and receivers.|
|Configure(CrossportSignaling)|*Optional* protected virtual method|Called after crossport signaling is created. It's normally used to start render streaming SingleConnection.|
|Awake()|*Optional* public virtual MonoBehaviour method|`MonoBehaviour.Awake()` |
|SignalingManager|**Required** SerializeField of base class|A render streaming SignalingManager, which should be attached to an instance in the unity Editor UI|
|defaultConfigName|*Optional* SerializeField of base class|Crossport setting filename prefix to be used, which can be configured in the Unity Editor UI. Normally, the setting file is the `${defaultConfigName}.cpcfg.json` file in the startup directory.|
|defaultConfig|*Optional* SerializeField of base class|The default config to use if crossport setting file is not found, which can be configured in the Unity Editor UI|
|disabled|*Optional* SerializeField of base class|Disable the drive. It can be configured in the Unity Editor UI.|

Here is an example for sender drivers that are required for all remote renderer applications:

```csharp
public class ExampleSenderDriver : CrossportDriverBase
{
#pragma warning disable 0649
    [SerializeField] private AudioStreamSender gameAudio;
    [SerializeField] private VideoStreamSender mainCamera;
#pragma warning restore 0649
    protected override void Configure(CrossportSetting setting)
    {
        if (setting is null) return;
        setting.Video(nameof(mainCamera))?.Configure(mainCamera);
        setting.Audio(nameof(gameAudio))?.Configure(gameAudio);
    }
}
```

## Write the crossport setting file

Create a `${defaultConfigName}.cpcfg.json` file in the root directory of your project. The format is:

```json
{
    "video": [
        {
            "key": "mainCamera",
            "codec": {
                "m_MimeType": "video/VP8",
                "m_SdpFmtpLine": "implementation_name=Internal"
            },
            "minBitrate": 0,
            "maxBitrate": 10000,
            "frameRate": 30.0,
            "resolutionX": 2560,
            "resolutionY": 1440
        }
    ],
    "audio": [
        {
            "key": "gameAudio",
            "codec": {
                "m_MimeType": "audio/opus",
                "m_SdpFmtpLine": "minptime=10;sprop-stereo=1;stereo=1;useinbandfec=1",
                "m_ChannelCount": 2,
                "m_SampleRate": 48000
            },
            "minBitrate": 0,
            "maxBitrate": 2000
        }
    ],
    "signaling": {
        "application": "${APP_NAME}",
        "capacity": ${CAPACITY},
        "component": "${COMP_NAME}",
        "address": "${SERVER_NAME}",
        "interval": 5.0
    }
}
```

### `video` Section

This is an array of video settings.

- `key`: Key of the video setting, normally set to `nameof(VideoStreamSender/VideoStreamReceiver)`.
- `codec`: Codec info of video stream. By design, RenderStreaming is supposed to support multiple video codecs, but actually only supports VP8 on linux machines, so **just left it AS IS**.
- `minBitrate`: Minimum bitrate of video stream, for **senders only**.
- `maxBitrate`: Maximum bitrate of video stream, for **senders only**.
- `frameRate`: Maximum framerate of video stream, for **senders only**.
- `resolutionX` and `resolutionY`: Resolution of video stream, for **senders only**.

### `audio` Section

This is an array of audio settings.

- `key`: Key of the audio setting, normally set to `nameof(AudioStreamSender/AudioStreamReceiver)`.
- `codec`: Codec info of audio stream. You might just left it AS IS.
- `minBitrate`: Minimum bitrate of audio stream, for **senders only**.
- `maxBitrate`: Maximum bitrate of audio stream, for **senders only**.

### `signaling` Section

- `application`: Application Name.
- `capacity`: Maximum number of clients the renderer supposed to serve. Set it to zero for clients (stream receivers).
- `component`: Component Name.
- `address`: Address of the Crossport Server. This is a websocket url, e.g., `ws://localhost/sig`.
- `interval`: Websocket keep-alive interval. You might just left it AS IS.

### Additional commandline arguments for standalone build

They're parsed by `CrossportConfigurationManager` and `CrossportDriverBase.Awake()` automatically. Commandline arguments don't work with Unity Editor.

- `-cpd <dir>`: *Optional.* Change Crossport Setting dir, default is the startup dir.
- `-c:${defaultConfigName} <newConfigName>`: *Optional.* Change Crossport Setting filename from `${defaultConfigName}.cpcfg.json` to `${newConfigName}.cpcfg.json`.
- `-ph`: *Optional.* Enable the `${PLACE_HOLDER}` resolving in Crossport Setting files. The `${PLACE_HOLDER}` will be solved by environment variables, so this argument is usually used in container builds.

**Now, start your Application (in Unity Editor or Standalone Build) and check out the client. It should be running well.**
