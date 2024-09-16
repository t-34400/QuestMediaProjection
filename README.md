# Quest Media Projection Plugin for Unity

`Quest Media Projection Plugin for Unity` is a Unity plugin for capturing the Meta Quest screen using the MediaProjection API. This plugin provides three main features:

1. **Capture Screen as Texture2D**: Capture the Meta Quest screen and handle it as a `Texture2D` in Unity.
2. **Barcode Scanning**: Read specified barcodes from the captured screen using ZXing.
3. **Save Screen Captures**: Save the captured screen to storage.

## YouTube
<div align="left">
  <a href="https://www.youtube.com/watch?v=cM_gVy-KmuM"><img src="https://img.youtube.com/vi/cM_gVy-KmuM/0.jpg" alt="IMAGE ALT TEXT"></a>
</div>
<div align="left">
  <a href="https://www.youtube.com/watch?v=RuTvhjlL4pQ"><img src="https://img.youtube.com/vi/RuTvhjlL4pQ/0.jpg" alt="IMAGE ALT TEXT"></a>
</div>

## Installation

1. **Create a Meta Quest Project**: Refer to the official tutorial at [Oculus Developer Documentation](https://developer.oculus.com/documentation/unity/unity-tutorial-hello-vr/) to set up a project for Meta Quest.

2. **Download and Import UnityPackage**:
   - Download the UnityPackage from the [GitHub Releases](https://github.com/t-34400/QuestMediaProjection/releases) page.
   - Import the `.unitypackage` into your Unity project.

3. **Configure Project Settings**:
   - Go to the menu bar and select `Edit > Project Settings`.
   - In the window that appears, go to the `Player` tab and configure the following:
     - In the `Other Settings` panel, set the `Target API Level` to 34 or higher.
     - In the `Publishing Settings`, check the boxes for `Custom Main Manifest`, `Custom Main Gradle Template`, and `Custom Gradle Properties Template`.
     <p float="left">
     <img src="Images/player-tab_1.png" width="300" />
     <img src="Images/player-tab_2.png" width="300" />
     </p>

4. **Modify AndroidManifest.xml**:
   - Open `Assets/Plugins/Android/AndroidManifest.xml`.
   - Add the following tag within the `manifest` tag:
     ```xml
     <manifest ...>
         <uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
         ...
     ```
   - Add the following tag within the `application` tag:
     ```xml
     <application ...>
        <service
            android:name="com.t34400.mediaprojectionlib.core.MediaProjectionService"
            android:foregroundServiceType="mediaProjection"
            android:stopWithTask="true" />
        ...
     </application>
     ```

5. **Update gradleTemplate.properties**:
   - Open `Assets/Plugins/Android/gradleTemplate.properties`.
   - Add the following lines:
     ```
     android.useAndroidX=true
     android.enableJetifier=true
     ```

6. **Update mainTemplate.gradle**:
   - Open `Assets/Plugins/Android/mainTemplate.gradle`.
   - Add the following dependencies in the `dependencies` scope:
     ```groovy
     dependencies {
        ...
        implementation 'androidx.appcompat:appcompat:1.6.1'
        implementation 'org.jetbrains.kotlinx:kotlinx-serialization-json:1.7.2'
        implementation 'com.google.zxing:core:3.5.3'
     }
     ```

## Usage

### Basic Setup

1. Add the `ServiceContainer` component and `MediaProjectionViewModel` component to a suitable `GameObject`.
2. In the `MediaProjectionViewModel` component, set the `ServiceContainer` field to the previously added `ServiceContainer` component.
3. Adjust the screen capture frequency using the `Min Update Interval [s]` field.
4. Proceed to configure any of the following features as needed.
<p float="left">
<img src="Images/base-components.png" width="300" />
</p>

### Capturing Screen as Texture2D

1. In the `MediaProjectionViewModel` component, check the `Texture Required` option.
2. Assign the object that will use the texture to the `Texture Updated` event and select the property/method from the dropdown.
   - If you want to process the texture in a custom component, define a public method like the following, attach it to a `GameObject`, register it with the event, and select the method from the dropdown:
     ```csharp
     using UnityEngine;

     class TextureHandler : MonoBehaviour
     {
         public void ProcessTexture(Texture2D texture)
         {
             // process texture...
         }
     }
     ```
   - If you want to apply the texture to a material, attach the material and select `mainTexture` from the dropdown.

### Reading Barcodes

1. Add the `BarcodeReaderViewModel` component to a suitable `GameObject` and attach the `MediaProjectionViewModel` component created in the basic setup to its `MediaProjectionViewModel` field.
2. Select the barcodes to be read from the `PossibleFormats` list (multiple formats can be selected). 
    <details><summary>Supported barcode formats:</summary>

    - `AZTEC`
    - `CODABAR`
    - `CODE_128`
    - `CODE_39`
    - `CODE_93`
    - `DATA_MATRIX`
    - `EAN_13`
    - `EAN_8`
    - `ITF`
    - `MAXICODE`
    - `PDF_417`
    - `QR_CODE`
    - `RSS_14`
    - `RSS_EXPANDED`
    - `UPC_A`
    - `UPC_E`
    - `UPC_EAN_EXTENSION`
    </details>
3. To crop the input image before barcode reading, check `Crop Required` and specify the `Crop Rect`.
4. For higher accuracy, check `Try Harder`.
5. To handle barcode reading results, create a component and register it with the `Barcode Read` event:
   ```csharp
   using UnityEngine;
   using MediaProjection.Models;

   class ResultHandler : MonoBehaviour
   {
       public void ProcessResult(BarcodeReadingResult result)
       {
           string text = result.Text; // raw text encoded by the barcode
           string format = result.Format; // format of the barcode that was decoded
           byte[] rawBytes = result.RawBytes; // raw bytes encoded by the barcode
           Vector2[] resultPoints = result.ResultPoints; // points related to the barcode in the image
           long timestamp = result.Timestamp;

           // ...
       }
   }
<p float="left">
<img src="Images/barcode-reader-component.png" width="300" />
</p>

### Saving Captures

1. Add the `ImageSaverViewModel` component to a suitable `GameObject` and attach the `MediaProjectionViewModel` component created in the basic setup to its `MediaProjectionViewModel` field.
2. Specify a filename prefix in the `FilenamePrefix` field.
3. The captured images will be saved to `/sdcard/Android/data/<package name>/files/<FilenamePrefix><timestamp>.jpg`.
<p float="left">
<img src="Images/image-saver-component.png" width="300" />
</p>

## License

This project is licensed under the [MIT License](LICENSE).

## AAR Source Code
[MediaProjectionLib](https://github.com/t-34400/MediaProjectionLib)

## Acknowledgements

This project uses the [ZXing](https://github.com/zxing/zxing) library, which is licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0). 

We would like to thank the ZXing contributors for their work on this library.