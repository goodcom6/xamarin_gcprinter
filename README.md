# GcPrint for Xamarin

**GcPrint** is a lightweight Xamarin.Android printing library designed specifically for integration with Goodcom Android POS devices.

## 🧾 Sample Print Output

![GcPrint Sample Output](https://raw.githubusercontent.com/G308587806/GcPrintAssets/main/order-receipt.jpg)

## ✨ Features

- ✅ Print using native Android AIDL APIs  
- ✅ Supports barcode, QR code, and custom text printing  
- ✅ Supports bitmap image printing (e.g. logos, receipts)  
- ✅ Simple API and easy integration in Xamarin.Android projects  
- ✅ Minimal dependencies  


## 📦 Installation

Install via NuGet:

```
Install-Package Xamarin.Goodcom.GcPrint
```

Or using `.csproj`:

```xml
<PackageReference Include="Xamarin.Goodcom.GcPrint" Version="2.1.*" />
```

## 🚀 Usage

```csharp
using Com.Goodcom.GcPrint;
using Android.Graphics;

// Create an instance of the printer helper
var helper = GcPrinterHelper.Instance;

// Draw a QR code centered
helper.DrawBarcode("Test QrCode", GcPrinterHelper.AlignCenter, GcPrinterHelper.BarcodeQrCode);

// Draw a separator line
helper.DrawOneLine();

// Draw custom text aligned to the left
helper.DrawCustom("Easy print", 0, 0);

// Draw another separator line
helper.DrawOneLine();

// Draw custom text centered
helper.DrawCustom("Thanks!", 0, GcPrinterHelper.AlignCenter);

// Load a bitmap (from resources, file, or generated)
Bitmap bmp = BitmapFactory.DecodeResource(Resources, Resource.Drawable.sample_image);

// Print the bitmap centered
helper.PrintBitmap(ApplicationContext, bmp, GcPrinterHelper.AlignCenter, true);

// Start the print job
helper.PrintText(ApplicationContext, true);
```

## 📄 License

Licensed under the MIT License. See `LICENSE.txt` for details.

## 💬 Support

For any questions, issues, or feature requests, please contact us:

- 📧 **Email:** [support@igoodcom.com](mailto:support@igoodcom.com)  
- 🌐 **Website:** [https://www.igoodcom.com](https://www.igoodcom.com)