using System;
using System.Runtime.InteropServices;
using RuntimeHelpers = System.Runtime.CompilerServices.RuntimeHelpers;
using SecureString = System.Security.SecureString;

// From:
//    http://blogs.msdn.com/b/dcook/archive/2008/11/25/creating-a-self-signed-certificate-in-c.aspx

namespace ACMESharp {
 internal static class Certificate {
  public static Byte[] CreateSelfSignCertificatePfx (
      String x500,
      DateTime startTime,
      DateTime endTime ) {
   var pfxData = CreateSelfSignCertificatePfx (
       x500,
       startTime,
       endTime,
       (SecureString) null );
   return pfxData;
  }

  public static Byte[] CreateSelfSignCertificatePfx (
      String x500,
      DateTime startTime,
      DateTime endTime,
      String insecurePassword ) {
   Byte[] pfxData;
   SecureString password = null;

   try {
    if ( !String.IsNullOrEmpty ( insecurePassword ) ) {
     password = new SecureString ();
     foreach ( var ch in insecurePassword ) {
      password.AppendChar ( ch );
     }

     password.MakeReadOnly ();
    }

    pfxData = CreateSelfSignCertificatePfx (
        x500,
        startTime,
        endTime,
        password );
   } finally {
    if ( password != null ) {
     password.Dispose ();
    }
   }

   return pfxData;
  }

  public static Byte[] CreateSelfSignCertificatePfx (
      String x500,
      DateTime startTime,
      DateTime endTime,
      SecureString password ) {
   Byte[] pfxData;

   x500 = x500 ?? "";

   var startSystemTime = ToSystemTime ( startTime );
   var endSystemTime = ToSystemTime ( endTime );
   var containerName = Guid.NewGuid ().ToString ();

   var dataHandle = new GCHandle ();
   var providerContext = IntPtr.Zero;
   var cryptKey = IntPtr.Zero;
   var certContext = IntPtr.Zero;
   var certStore = IntPtr.Zero;
   var storeCertContext = IntPtr.Zero;
   var passwordPtr = IntPtr.Zero;
   RuntimeHelpers.PrepareConstrainedRegions ();
   try {
    Check ( NativeMethods.CryptAcquireContextW (
        out providerContext,
        containerName,
        null,
        1, // PROV_RSA_FULL
        8 ) ); // CRYPT_NEWKEYSET

    Check ( NativeMethods.CryptGenKey (
        providerContext,
        1, // AT_KEYEXCHANGE
        1, // CRYPT_EXPORTABLE
        out cryptKey ) );

    var nameDataLength = 0;
    Byte[] nameData;

    // errorStringPtr gets a pointer into the middle of the x500 string,
    // so x500 needs to be pinned until after we've copied the value
    // of errorStringPtr.
    dataHandle = GCHandle.Alloc ( x500, GCHandleType.Pinned );

    if ( !NativeMethods.CertStrToNameW (
        0x00010001, // X509_ASN_ENCODING | PKCS_7_ASN_ENCODING
        dataHandle.AddrOfPinnedObject (),
        3, // CERT_X500_NAME_STR = 3
        IntPtr.Zero,
        null,
        ref nameDataLength,
        out var errorStringPtr ) ) {
     var error = Marshal.PtrToStringUni ( errorStringPtr );
     throw new ArgumentException ( error );
    }

    nameData = new Byte[ nameDataLength ];

    if ( !NativeMethods.CertStrToNameW (
        0x00010001, // X509_ASN_ENCODING | PKCS_7_ASN_ENCODING
        dataHandle.AddrOfPinnedObject (),
        3, // CERT_X500_NAME_STR = 3
        IntPtr.Zero,
        nameData,
        ref nameDataLength,
        out errorStringPtr ) ) {
     var error = Marshal.PtrToStringUni ( errorStringPtr );
     throw new ArgumentException ( error );
    }

    dataHandle.Free ();

    dataHandle = GCHandle.Alloc ( nameData, GCHandleType.Pinned );
    var nameBlob = new CryptoApiBlob (
        nameData.Length,
        dataHandle.AddrOfPinnedObject () );

    var kpi = new CryptKeyProviderInformation () {
     ContainerName = containerName,
     ProviderType = 1, // PROV_RSA_FULL
     KeySpec = 1 // AT_KEYEXCHANGE
    };

    certContext = NativeMethods.CertCreateSelfSignCertificate (
        providerContext,
        ref nameBlob,
        0,
        ref kpi,
        IntPtr.Zero, // default = SHA1RSA
        ref startSystemTime,
        ref endSystemTime,
        IntPtr.Zero );
    Check ( certContext != IntPtr.Zero );
    dataHandle.Free ();

    certStore = NativeMethods.CertOpenStore (
        "Memory", // sz_CERT_STORE_PROV_MEMORY
        0,
        IntPtr.Zero,
        0x2000, // CERT_STORE_CREATE_NEW_FLAG
        IntPtr.Zero );
    Check ( certStore != IntPtr.Zero );

    Check ( NativeMethods.CertAddCertificateContextToStore (
        certStore,
        certContext,
        1, // CERT_STORE_ADD_NEW
        out storeCertContext ) );

    NativeMethods.CertSetCertificateContextProperty (
        storeCertContext,
        2, // CERT_KEY_PROV_INFO_PROP_ID
        0,
        ref kpi );

    if ( password != null ) {
     passwordPtr = Marshal.SecureStringToCoTaskMemUnicode ( password );
    }

    var pfxBlob = new CryptoApiBlob ();
    Check ( NativeMethods.PFXExportCertStoreEx (
        certStore,
        ref pfxBlob,
        passwordPtr,
        IntPtr.Zero,
        7 ) ); // EXPORT_PRIVATE_KEYS | REPORT_NO_PRIVATE_KEY | REPORT_NOT_ABLE_TO_EXPORT_PRIVATE_KEY

    pfxData = new Byte[ pfxBlob.DataLength ];
    dataHandle = GCHandle.Alloc ( pfxData, GCHandleType.Pinned );
    pfxBlob.Data = dataHandle.AddrOfPinnedObject ();
    Check ( NativeMethods.PFXExportCertStoreEx (
        certStore,
        ref pfxBlob,
        passwordPtr,
        IntPtr.Zero,
        7 ) ); // EXPORT_PRIVATE_KEYS | REPORT_NO_PRIVATE_KEY | REPORT_NOT_ABLE_TO_EXPORT_PRIVATE_KEY
    dataHandle.Free ();
   } finally {
    if ( passwordPtr != IntPtr.Zero ) {
     Marshal.ZeroFreeCoTaskMemUnicode ( passwordPtr );
    }

    if ( dataHandle.IsAllocated ) {
     dataHandle.Free ();
    }

    if ( certContext != IntPtr.Zero ) {
     NativeMethods.CertFreeCertificateContext ( certContext );
    }

    if ( storeCertContext != IntPtr.Zero ) {
     NativeMethods.CertFreeCertificateContext ( storeCertContext );
    }

    if ( certStore != IntPtr.Zero ) {
     NativeMethods.CertCloseStore ( certStore, 0 );
    }

    if ( cryptKey != IntPtr.Zero ) {
     NativeMethods.CryptDestroyKey ( cryptKey );
    }

    if ( providerContext != IntPtr.Zero ) {
     NativeMethods.CryptReleaseContext ( providerContext, 0 );
     NativeMethods.CryptAcquireContextW (
         out providerContext,
         containerName,
         null,
         1, // PROV_RSA_FULL
         0x10 ); // CRYPT_DELETEKEYSET
    }
   }

   return pfxData;
  }

  private static SystemTime ToSystemTime ( DateTime dateTime ) {
   var fileTime = dateTime.ToFileTime ();
   Check ( NativeMethods.FileTimeToSystemTime ( ref fileTime, out var systemTime ) );
   return systemTime;
  }

  private static void Check ( Boolean nativeCallSucceeded ) {
   if ( !nativeCallSucceeded ) {
    var error = Marshal.GetHRForLastWin32Error ();
    Marshal.ThrowExceptionForHR ( error );
   }
  }

  [StructLayout ( LayoutKind.Sequential )]
  private struct SystemTime {
   public Int16 Year;
   public Int16 Month;
   public Int16 DayOfWeek;
   public Int16 Day;
   public Int16 Hour;
   public Int16 Minute;
   public Int16 Second;
   public Int16 Milliseconds;
  }

  [StructLayout ( LayoutKind.Sequential )]
  private struct CryptoApiBlob {
   public Int32 DataLength;
   public IntPtr Data;

   public CryptoApiBlob ( Int32 dataLength, IntPtr data ) {
    DataLength = dataLength;
    Data = data;
   }
  }

  [StructLayout ( LayoutKind.Sequential )]
  private struct CryptKeyProviderInformation {
   [MarshalAs ( UnmanagedType.LPWStr )]
   public String ContainerName;

   [MarshalAs ( UnmanagedType.LPWStr )]
   public String ProviderName;

   public Int32 ProviderType;
   public Int32 Flags;
   public Int32 ProviderParameterCount;
   public IntPtr ProviderParameters; // PCRYPT_KEY_PROV_PARAM
   public Int32 KeySpec;
  }

  private static class NativeMethods {
   [DllImport ( "kernel32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean FileTimeToSystemTime (
       [In] ref Int64 fileTime,
       out SystemTime systemTime );

   [DllImport ( "AdvApi32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean CryptAcquireContextW (
       out IntPtr providerContext,
       [MarshalAs ( UnmanagedType.LPWStr )] String container,
       [MarshalAs ( UnmanagedType.LPWStr )] String provider,
       Int32 providerType,
       Int32 flags );

   [DllImport ( "AdvApi32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean CryptReleaseContext (
       IntPtr providerContext,
       Int32 flags );

   [DllImport ( "AdvApi32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean CryptGenKey (
       IntPtr providerContext,
       Int32 algorithmId,
       Int32 flags,
       out IntPtr cryptKeyHandle );

   [DllImport ( "AdvApi32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean CryptDestroyKey (
       IntPtr cryptKeyHandle );

   [DllImport ( "Crypt32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean CertStrToNameW (
       Int32 certificateEncodingType,
       IntPtr x500,
       Int32 strType,
       IntPtr reserved,
       [MarshalAs ( UnmanagedType.LPArray )] [Out] Byte[] encoded,
       ref Int32 encodedLength,
       out IntPtr errorString );

   [DllImport ( "Crypt32.dll", SetLastError = true, ExactSpelling = true )]
   public static extern IntPtr CertCreateSelfSignCertificate (
       IntPtr providerHandle,
       [In] ref CryptoApiBlob subjectIssuerBlob,
       Int32 flags,
       [In] ref CryptKeyProviderInformation keyProviderInformation,
       IntPtr signatureAlgorithm,
       [In] ref SystemTime startTime,
       [In] ref SystemTime endTime,
       IntPtr extensions );

   [DllImport ( "Crypt32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean CertFreeCertificateContext (
       IntPtr certificateContext );

   [DllImport ( "Crypt32.dll", SetLastError = true, ExactSpelling = true )]
   public static extern IntPtr CertOpenStore (
       [MarshalAs ( UnmanagedType.LPStr )] String storeProvider,
       Int32 messageAndCertificateEncodingType,
       IntPtr cryptProvHandle,
       Int32 flags,
       IntPtr parameters );

   [DllImport ( "Crypt32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean CertCloseStore (
       IntPtr certificateStoreHandle,
       Int32 flags );

   [DllImport ( "Crypt32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean CertAddCertificateContextToStore (
       IntPtr certificateStoreHandle,
       IntPtr certificateContext,
       Int32 addDisposition,
       out IntPtr storeContextPtr );

   [DllImport ( "Crypt32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean CertSetCertificateContextProperty (
       IntPtr certificateContext,
       Int32 propertyId,
       Int32 flags,
       [In] ref CryptKeyProviderInformation data );

   [DllImport ( "Crypt32.dll", SetLastError = true, ExactSpelling = true )]
   [return: MarshalAs ( UnmanagedType.Bool )]
   public static extern Boolean PFXExportCertStoreEx (
       IntPtr certificateStoreHandle,
       ref CryptoApiBlob pfxBlob,
       IntPtr password,
       IntPtr reserved,
       Int32 flags );
  }
 }
}