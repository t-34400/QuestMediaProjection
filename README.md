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

## Device Preparation

### 1. **Firmware Version**
Ensure that your Meta Quest device is running **firmware v68 or later**.  
Media Projection functionality was re-enabled starting with **firmware v68**, so earlier versions may not work as expected.

### 2. **App Permissions - Spatial Data**  
Make sure to enable the **Spatial Data** permission for the app to function correctly.  
To do this:
  1. Open **Settings** on your Quest device.
  2. Navigate to **Apps > App Permissions**.
  3. Find your app and ensure **Spatial Data** is enabled.

*(Thanks to [anagpuyol](https://github.com/t-34400/QuestMediaProjection/issues/2#issuecomment-2677194000) for pointing this out!)*  

## Installation

1. **Create a Meta Quest Project**:  
   - If using the **Meta SDK**, refer to the official tutorials:  
     - [Hello World](https://developers.meta.com/horizon/documentation/unity/unity-tutorial-hello-vr)  
     - [Passthrough Starter Guide](https://developers.meta.com/horizon/documentation/unity/unity-passthrough-gs)  
   - If using **OpenXR**, refer to the OpenXR implementation at:  
     - [QuestMediaProjection-OpenXR](https://github.com/t-34400/QuestMediaProjection-OpenXR)  

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
    
  - If you have enabled `Minify` in the Publishing Settings, you will need to check the `Custom Proguard File` option.

4. **Modify AndroidManifest.xml**:  
   - Open `Assets/Plugins/Android/AndroidManifest.xml`.  
   - Add the following permission inside the `<manifest>` tag:  
     ```xml
     <manifest ...>
         <uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
         <uses-permission android:name="android.permission.FOREGROUND_SERVICE_MEDIA_PROJECTION" />
         ...
     </manifest>
     ```
   - Add the following service definition inside the `<application>` tag:  
     ```xml
     <application ...>
        <service
            android:name="com.t34400.mediaprojectionlib.core.MediaProjectionService"
            android:foregroundServiceType="mediaProjection"
            android:stopWithTask="true"
            android:exported="false" />
        ...
     </application>
     ```
   - **Note:** In **Unity 6+**, you need to **remove the `UnityPlayerActivity` block** from the manifest to avoid conflicts.  
     If your project is using **GameActivity**, keep the `UnityPlayerGameActivity` block and remove the `UnityPlayerActivity` block:  
     ```xml
     <!-- Remove this block if using GameActivity -->
     <activity android:name="com.unity3d.player.UnityPlayerActivity" ...>
         ...
     </activity>

     <!-- Keep this block if using GameActivity -->
     <activity android:name="com.unity3d.player.UnityPlayerGameActivity" ...>
         ...
     </activity>
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
   - Add the appropriate dependencies in the `dependencies` scope:
     ```groovy
     dependencies {
        ...
        implementation 'androidx.appcompat:appcompat:1.6.1'
        implementation 'org.jetbrains.kotlinx:kotlinx-serialization-json:1.7.2'

        implementation 'com.google.zxing:core:3.5.3'  // Use this if you are using ZXing for barcode scanning

        implementation 'com.google.mlkit:barcode-scanning:17.3.0'  // Use this if you are using Google ML Kit for barcode scanning
     }
     ```

7. **Update proguard-user.txt**:
  - If you have enabled Minify in the Publishing Settings you need to add the following line to the generated `Assets/Plugins/Android/proguard-user.txt` by enabling the `Custom Proguard File` option 
    ```txt
    -keep class com.t34400.mediaprojectionlib.** { *; }
    ```
    (Thank you to [stephanmitph](https://github.com/t-34400/QuestMediaProjection/issues/5) for pointing out!)

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

### Reading Barcodes with Zxing

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
5. To handle barcode reading results, create a component and register it with the `Barcode Read` event. You need to define a public method in your component that takes an array of `BarcodeReadingResult[]` as an argument:
   ```csharp
   using UnityEngine;
   using MediaProjection.Models;

   class ResultHandler : MonoBehaviour
   {
       public void ProcessResult(BarcodeReadingResult[] results)
       {
           foreach (var result in results)
           {
              　string text = result.Text; // raw text encoded by the barcode
              　string format = result.Format; // format of the barcode that was decoded
              　byte[] rawBytes = result.RawBytes; // raw bytes encoded by the barcode
              　Vector2[] resultPoints = result.ResultPoints; // points related to the barcode in the image
              　long timestamp = result.Timestamp;
              　
              　// ...
           }
       }
   }
   ```
<p float="left">
<img src="Images/barcode-reader-component.png" width="300" />
</p>

### Reading Barcodes with Google MLKit

1. Add the `MlKitBarcodeReaderViewModel` component to a suitable `GameObject` and attach the `MediaProjectionViewModel` component created in the basic setup to its `MediaProjectionViewModel` field.
2. Select the barcodes to be read from the `PossibleFormats` list (multiple formats can be selected). 
    <details><summary>Supported barcode formats:</summary>

    - `CODE_128`
    - `CODE_39`
    - `CODE_93`
    - `CODABAR`
    - `DATA_MATRIX`
    - `EAN_13`
    - `EAN_8`
    - `ITF`
    - `QR_CODE`
    - `UPC_A`
    - `UPC_E`
    - `PDF417`
    - `AZTEC`
    </details>
3. To handle barcode reading results, create a component and register it with the `Barcode Read` event. Similar to ZXing, you need to define a public method in your component that takes an array of `BarcodeReadingResult[]` as an argument.
<p float="left">
<img src="Images/mlkit-barcode-reader-component.png" width="300" />
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