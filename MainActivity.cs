using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Com.Goodcom.Gcprinter;
using Java.IO;
using System;
using Android.Net;
using System.IO;
using Android.Util;

namespace GcPrintX
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        const int OPEN_PICTURE = 1;
        const int PERMISSION_REQUEST_CODE = 100;

        EditText mTvTitle;
        EditText mEdText;
        EditText mTvItem1Number;
        EditText mTvItem1Name;
        EditText mTvItem1Amt;
        EditText mTvItem1Opt1Name;
        EditText mTvItem1Opt1Amt;
        EditText mTvItem1Opt2Name;
        EditText mTvItem1Opt2Amt;
        EditText mTvItem2Number;
        EditText mTvItem2Name;
        EditText mTvItem2Amt;
        EditText mTvItem2Opt1Name;
        EditText mTvItem2Opt1Amt;
        EditText mTvBarcode;
        Button mBtnBmp;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            mTvTitle = FindViewById<EditText>(Resource.Id.ticket_title);
            mTvItem1Number = FindViewById<EditText>(Resource.Id.item1_left);
            mTvItem1Name = FindViewById<EditText>(Resource.Id.item1_mid);
            mTvItem1Amt = FindViewById<EditText>(Resource.Id.item1_right);
            mTvItem2Number = FindViewById<EditText>(Resource.Id.item2_left);
            mTvItem2Name = FindViewById<EditText>(Resource.Id.item2_mid);
            mTvItem2Amt = FindViewById<EditText>(Resource.Id.item2_right);
            mTvItem1Opt1Name = FindViewById<EditText>(Resource.Id.item1_opt1_name);
            mTvItem1Opt1Amt = FindViewById<EditText>(Resource.Id.item1_opt1_amt);
            mTvItem1Opt2Name = FindViewById<EditText>(Resource.Id.item1_opt2_name);
            mTvItem1Opt2Amt = FindViewById<EditText>(Resource.Id.item1_opt2_amt);
            mTvItem2Opt1Name = FindViewById<EditText>(Resource.Id.item2_opt1_name);
            mTvItem2Opt1Amt = FindViewById<EditText>(Resource.Id.item2_opt1_amt);
            mTvBarcode = FindViewById<EditText>(Resource.Id.barcode);

            mEdText = FindViewById<EditText>(Resource.Id.ed_text);
            var btPrint = FindViewById<Button>(Resource.Id.btn_print);
            mBtnBmp = FindViewById<Button>(Resource.Id.btn_bmp);

            btPrint.Click += (s, e) =>
            {
                var helper = GcPrinterHelper.Instance;

                helper.DrawCustom(mTvTitle.Text, GcPrinterHelper.FontBig, GcPrinterHelper.AlignCenter);
                helper.DrawOneLine();
                helper.DrawNewLine();

                helper.DrawText(mTvItem1Number.Text, GcPrinterHelper.FontSmallBold,
                                mTvItem1Name.Text, GcPrinterHelper.FontSmallBold,
                                mTvItem1Amt.Text, GcPrinterHelper.FontSmallBold);

                helper.DrawLeftRight(mTvItem1Opt1Name.Text, 0,
                                     mTvItem1Opt1Amt.Text, 0);

                helper.DrawLeftRight(mTvItem1Opt2Name.Text, 0,
                                     mTvItem1Opt2Amt.Text, 0);

                helper.DrawNewLine();

                helper.DrawText(mTvItem2Number.Text, GcPrinterHelper.FontSmallBold,
                                mTvItem2Name.Text, GcPrinterHelper.FontSmallBold,
                                mTvItem2Amt.Text, GcPrinterHelper.FontSmallBold);

                helper.DrawLeftRight(mTvItem2Opt1Name.Text, 0,
                                     mTvItem2Opt1Amt.Text, 0);

                helper.DrawNewLine();

                helper.DrawBarcode(mTvBarcode.Text, GcPrinterHelper.AlignCenter, GcPrinterHelper.BarcodeQrCode);
                helper.DrawOneLine();
                helper.DrawCustom(mEdText.Text, 0, 0);
                helper.DrawOneLine();
                helper.DrawCustom("Thanks!", 0, GcPrinterHelper.AlignCenter);
                helper.PrintText(ApplicationContext, true);
            };

            mBtnBmp.Click += (s, e) =>
            {
                Intent intent = new Intent(Intent.ActionGetContent);
                intent.SetType("image/*");
                intent.AddCategory(Intent.CategoryOpenable);
                StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), OPEN_PICTURE);
            };

        }

        protected override void OnStart()
        {
            base.OnStart();
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, PERMISSION_REQUEST_CODE);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == OPEN_PICTURE && resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri uri = data.Data;
                Bitmap bmp = null;
                string path = null;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    bmp = ReadBmpApiQ(this, uri);
                }
                else
                {
                    if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
                    {
                        path = uri.Path;
                    }
                    else if (Build.VERSION.SdkInt > BuildVersionCodes.Kitkat)
                    {
                        path = GetPath(this, uri);
                    }
                    else
                    {
                        path = GetRealPathFromURI(uri);
                    }
                    if (!string.IsNullOrEmpty(path))
                    {
                        bmp = GetLocalBitmap(path);
                    }
                }

                if (bmp != null)
                {
                    GcPrinterHelper.Instance.PrintBitmap(ApplicationContext, bmp, GcPrinterHelper.AlignCenter, true);
                }
                else
                {
                    Toast.MakeText(this, "Fail to read picture file", ToastLength.Short).Show();
                }
            }
        }

        string GetRealPathFromURI(Android.Net.Uri contentUri)
        {
            string res = null;
            string[] proj = { MediaStore.Images.Media.InterfaceConsts.Data };
            using (ICursor cursor = ContentResolver.Query(contentUri, proj, null, null, null))
            {
                if (cursor != null && cursor.MoveToFirst())
                {
                    int column_index = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                    res = cursor.GetString(column_index);
                }
            }
            return res;
        }

        string GetPath(Context context, Android.Net.Uri uri)
        {
            bool isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            if (isKitKat && DocumentsContract.IsDocumentUri(context, uri))
            {
                if (IsExternalStorageDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(':');
                    string type = split[0];

                    if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }
                }
                else if (IsDownloadsDocument(uri))
                {
                    string id = DocumentsContract.GetDocumentId(uri);
                    Android.Net.Uri contentUri = ContentUris.WithAppendedId(
                        Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    return GetDataColumn(context, contentUri, null, null);
                }
                else if (IsMediaDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(':');
                    string type = split[0];

                    Android.Net.Uri contentUri = null;
                    switch (type)
                    {
                        case "image":
                            contentUri = MediaStore.Images.Media.ExternalContentUri;
                            break;
                        case "video":
                            contentUri = MediaStore.Video.Media.ExternalContentUri;
                            break;
                        case "audio":
                            contentUri = MediaStore.Audio.Media.ExternalContentUri;
                            break;
                    }

                    string selection = "_id=?";
                    string[] selectionArgs = new string[] { split[1] };

                    return GetDataColumn(context, contentUri, selection, selectionArgs);
                }
            }
            else if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return GetDataColumn(context, uri, null, null);
            }
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }
            return null;
        }

        string GetDataColumn(Context context, Android.Net.Uri uri, string selection, string[] selectionArgs)
        {
            string column = "_data";
            string[] projection = { column };
            using (ICursor cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null))
            {
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            return null;
        }

        bool IsExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        bool IsDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        bool IsMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        static Bitmap ReadBmpApiQ(Context context, Android.Net.Uri uri)
        {
            if (ContentResolver.SchemeFile.Equals(uri.Scheme))
            {
                try
                {
                    using (var fs = System.IO.File.OpenRead(uri.Path))
                    {
                        return BitmapFactory.DecodeStream(fs);
                    }
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Log.Debug("GcPrintX", e.ToString());
                }
                return null;
            }
            else if (ContentResolver.SchemeContent.Equals(uri.Scheme))
            {
                ContentResolver contentResolver = context.ContentResolver;
                try
                {
                    using (var input = contentResolver.OpenInputStream(uri))
                    {
                        return BitmapFactory.DecodeStream(input);
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("GcPrintX", e.ToString());
                }
            }
            return null;
        }

        static Bitmap GetLocalBitmap(string path)
        {
                try
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        return BitmapFactory.DecodeStream(stream);
                    }
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Log.Debug("GcPrintX", e.ToString());
                    return null;
                }
                catch (System.Exception ex)
                {
                    Log.Debug("GcPrintX", ex.ToString());
                    return null;
                }
        }

    }
}
