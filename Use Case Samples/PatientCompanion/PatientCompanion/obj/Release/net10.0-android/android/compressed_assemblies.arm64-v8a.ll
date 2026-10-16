; ModuleID = 'compressed_assemblies.arm64-v8a.ll'
source_filename = "compressed_assemblies.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.CompressedAssemblyDescriptor = type {
	i32, ; uint32_t uncompressed_file_size
	i1, ; bool loaded
	i32 ; uint32_t buffer_offset
}

@compressed_assembly_count = dso_local local_unnamed_addr constant i32 148, align 4

@compressed_assembly_descriptors = dso_local local_unnamed_addr global [148 x %struct.CompressedAssemblyDescriptor] [
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 0; uint32_t buffer_offset
	}, ; 0: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15144; uint32_t buffer_offset
	}, ; 1: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 30288; uint32_t buffer_offset
	}, ; 2: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 45432; uint32_t buffer_offset
	}, ; 3: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 60576; uint32_t buffer_offset
	}, ; 4: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 75760; uint32_t buffer_offset
	}, ; 5: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 90904; uint32_t buffer_offset
	}, ; 6: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 106048; uint32_t buffer_offset
	}, ; 7: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 121232; uint32_t buffer_offset
	}, ; 8: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 136376; uint32_t buffer_offset
	}, ; 9: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 151520; uint32_t buffer_offset
	}, ; 10: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 166704; uint32_t buffer_offset
	}, ; 11: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 181848; uint32_t buffer_offset
	}, ; 12: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 196992; uint32_t buffer_offset
	}, ; 13: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 212136; uint32_t buffer_offset
	}, ; 14: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 227320; uint32_t buffer_offset
	}, ; 15: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 242464; uint32_t buffer_offset
	}, ; 16: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 257608; uint32_t buffer_offset
	}, ; 17: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 272752; uint32_t buffer_offset
	}, ; 18: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 287896; uint32_t buffer_offset
	}, ; 19: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 303040; uint32_t buffer_offset
	}, ; 20: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 318184; uint32_t buffer_offset
	}, ; 21: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 333368; uint32_t buffer_offset
	}, ; 22: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 348512; uint32_t buffer_offset
	}, ; 23: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 363656; uint32_t buffer_offset
	}, ; 24: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 378800; uint32_t buffer_offset
	}, ; 25: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 393944; uint32_t buffer_offset
	}, ; 26: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 409088; uint32_t buffer_offset
	}, ; 27: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 424232; uint32_t buffer_offset
	}, ; 28: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 439416; uint32_t buffer_offset
	}, ; 29: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 454600; uint32_t buffer_offset
	}, ; 30: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 469744; uint32_t buffer_offset
	}, ; 31: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 484888; uint32_t buffer_offset
	}, ; 32: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 500032; uint32_t buffer_offset
	}, ; 33: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 4608, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 515216; uint32_t buffer_offset
	}, ; 34: Syncfusion.Maui.Buttons.resources
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 519824; uint32_t buffer_offset
	}, ; 35: _Microsoft.Android.Resource.Designer
	%struct.CompressedAssemblyDescriptor {
		i32 22528, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 526480; uint32_t buffer_offset
	}, ; 36: CommunityToolkit.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 36352, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 549008; uint32_t buffer_offset
	}, ; 37: CommunityToolkit.Maui.Core
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 585360; uint32_t buffer_offset
	}, ; 38: CommunityToolkit.Mvvm
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 604816; uint32_t buffer_offset
	}, ; 39: Microsoft.Extensions.Configuration
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 619664; uint32_t buffer_offset
	}, ; 40: Microsoft.Extensions.Configuration.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 46592, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 626320; uint32_t buffer_offset
	}, ; 41: Microsoft.Extensions.DependencyInjection
	%struct.CompressedAssemblyDescriptor {
		i32 26624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 672912; uint32_t buffer_offset
	}, ; 42: Microsoft.Extensions.DependencyInjection.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 8192, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 699536; uint32_t buffer_offset
	}, ; 43: Microsoft.Extensions.Diagnostics.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 707728; uint32_t buffer_offset
	}, ; 44: Microsoft.Extensions.FileProviders.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 714896; uint32_t buffer_offset
	}, ; 45: Microsoft.Extensions.Hosting.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 721040; uint32_t buffer_offset
	}, ; 46: Microsoft.Extensions.Logging
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 738960; uint32_t buffer_offset
	}, ; 47: Microsoft.Extensions.Logging.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 16896, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 758416; uint32_t buffer_offset
	}, ; 48: Microsoft.Extensions.Options
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 775312; uint32_t buffer_offset
	}, ; 49: Microsoft.Extensions.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 1980712, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 784528; uint32_t buffer_offset
	}, ; 50: Microsoft.Maui.Controls
	%struct.CompressedAssemblyDescriptor {
		i32 135976, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2765240; uint32_t buffer_offset
	}, ; 51: Microsoft.Maui.Controls.Xaml
	%struct.CompressedAssemblyDescriptor {
		i32 966144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2901216; uint32_t buffer_offset
	}, ; 52: Microsoft.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 67072, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3867360; uint32_t buffer_offset
	}, ; 53: Microsoft.Maui.Essentials
	%struct.CompressedAssemblyDescriptor {
		i32 210256, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3934432; uint32_t buffer_offset
	}, ; 54: Microsoft.Maui.Graphics
	%struct.CompressedAssemblyDescriptor {
		i32 65024, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4144688; uint32_t buffer_offset
	}, ; 55: Syncfusion.Licensing
	%struct.CompressedAssemblyDescriptor {
		i32 126464, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4209712; uint32_t buffer_offset
	}, ; 56: Syncfusion.Maui.Buttons
	%struct.CompressedAssemblyDescriptor {
		i32 20992, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4336176; uint32_t buffer_offset
	}, ; 57: Syncfusion.Maui.Cards
	%struct.CompressedAssemblyDescriptor {
		i32 13312, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4357168; uint32_t buffer_offset
	}, ; 58: Syncfusion.Maui.Carousel
	%struct.CompressedAssemblyDescriptor {
		i32 399360, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4370480; uint32_t buffer_offset
	}, ; 59: Syncfusion.Maui.Charts
	%struct.CompressedAssemblyDescriptor {
		i32 1179648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4769840; uint32_t buffer_offset
	}, ; 60: Syncfusion.Maui.Core
	%struct.CompressedAssemblyDescriptor {
		i32 140800, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5949488; uint32_t buffer_offset
	}, ; 61: Syncfusion.Maui.DataSource
	%struct.CompressedAssemblyDescriptor {
		i32 61952, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6090288; uint32_t buffer_offset
	}, ; 62: Syncfusion.Maui.GridCommon
	%struct.CompressedAssemblyDescriptor {
		i32 211456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6152240; uint32_t buffer_offset
	}, ; 63: Syncfusion.Maui.ListView
	%struct.CompressedAssemblyDescriptor {
		i32 185856, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6363696; uint32_t buffer_offset
	}, ; 64: Syncfusion.Maui.Maps
	%struct.CompressedAssemblyDescriptor {
		i32 89600, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6549552; uint32_t buffer_offset
	}, ; 65: Syncfusion.Maui.Popup
	%struct.CompressedAssemblyDescriptor {
		i32 81920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6639152; uint32_t buffer_offset
	}, ; 66: Syncfusion.Maui.ProgressBar
	%struct.CompressedAssemblyDescriptor {
		i32 58368, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6721072; uint32_t buffer_offset
	}, ; 67: Syncfusion.Telemetry
	%struct.CompressedAssemblyDescriptor {
		i32 73728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6779440; uint32_t buffer_offset
	}, ; 68: Xamarin.AndroidX.Activity
	%struct.CompressedAssemblyDescriptor {
		i32 595968, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6853168; uint32_t buffer_offset
	}, ; 69: Xamarin.AndroidX.AppCompat
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7449136; uint32_t buffer_offset
	}, ; 70: Xamarin.AndroidX.AppCompat.AppCompatResources
	%struct.CompressedAssemblyDescriptor {
		i32 50176, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7467056; uint32_t buffer_offset
	}, ; 71: Xamarin.AndroidX.Browser
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7517232; uint32_t buffer_offset
	}, ; 72: Xamarin.AndroidX.CardView
	%struct.CompressedAssemblyDescriptor {
		i32 22528, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7536176; uint32_t buffer_offset
	}, ; 73: Xamarin.AndroidX.Collection.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 78336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7558704; uint32_t buffer_offset
	}, ; 74: Xamarin.AndroidX.CoordinatorLayout
	%struct.CompressedAssemblyDescriptor {
		i32 597504, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7637040; uint32_t buffer_offset
	}, ; 75: Xamarin.AndroidX.Core
	%struct.CompressedAssemblyDescriptor {
		i32 26624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8234544; uint32_t buffer_offset
	}, ; 76: Xamarin.AndroidX.CursorAdapter
	%struct.CompressedAssemblyDescriptor {
		i32 9728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8261168; uint32_t buffer_offset
	}, ; 77: Xamarin.AndroidX.CustomView
	%struct.CompressedAssemblyDescriptor {
		i32 47104, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8270896; uint32_t buffer_offset
	}, ; 78: Xamarin.AndroidX.DrawerLayout
	%struct.CompressedAssemblyDescriptor {
		i32 236032, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8318000; uint32_t buffer_offset
	}, ; 79: Xamarin.AndroidX.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 23552, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8554032; uint32_t buffer_offset
	}, ; 80: Xamarin.AndroidX.Lifecycle.Common.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8577584; uint32_t buffer_offset
	}, ; 81: Xamarin.AndroidX.Lifecycle.LiveData.Core
	%struct.CompressedAssemblyDescriptor {
		i32 32768, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8596528; uint32_t buffer_offset
	}, ; 82: Xamarin.AndroidX.Lifecycle.ViewModel.Android
	%struct.CompressedAssemblyDescriptor {
		i32 13824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8629296; uint32_t buffer_offset
	}, ; 83: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 39424, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8643120; uint32_t buffer_offset
	}, ; 84: Xamarin.AndroidX.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 92672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8682544; uint32_t buffer_offset
	}, ; 85: Xamarin.AndroidX.Navigation.Common.Android
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8775216; uint32_t buffer_offset
	}, ; 86: Xamarin.AndroidX.Navigation.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 65536, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8794672; uint32_t buffer_offset
	}, ; 87: Xamarin.AndroidX.Navigation.Runtime.Android
	%struct.CompressedAssemblyDescriptor {
		i32 27136, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8860208; uint32_t buffer_offset
	}, ; 88: Xamarin.AndroidX.Navigation.UI
	%struct.CompressedAssemblyDescriptor {
		i32 457728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8887344; uint32_t buffer_offset
	}, ; 89: Xamarin.AndroidX.RecyclerView
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9345072; uint32_t buffer_offset
	}, ; 90: Xamarin.AndroidX.SavedState.SavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 41984, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9357360; uint32_t buffer_offset
	}, ; 91: Xamarin.AndroidX.SwipeRefreshLayout
	%struct.CompressedAssemblyDescriptor {
		i32 62976, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9399344; uint32_t buffer_offset
	}, ; 92: Xamarin.AndroidX.ViewPager
	%struct.CompressedAssemblyDescriptor {
		i32 40448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9462320; uint32_t buffer_offset
	}, ; 93: Xamarin.AndroidX.ViewPager2
	%struct.CompressedAssemblyDescriptor {
		i32 982016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9502768; uint32_t buffer_offset
	}, ; 94: Xamarin.Google.Android.Material
	%struct.CompressedAssemblyDescriptor {
		i32 90624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10484784; uint32_t buffer_offset
	}, ; 95: Xamarin.Kotlin.StdLib
	%struct.CompressedAssemblyDescriptor {
		i32 28672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10575408; uint32_t buffer_offset
	}, ; 96: Xamarin.KotlinX.Coroutines.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 91648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10604080; uint32_t buffer_offset
	}, ; 97: Xamarin.KotlinX.Serialization.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 245248, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10695728; uint32_t buffer_offset
	}, ; 98: PatientCompanion
	%struct.CompressedAssemblyDescriptor {
		i32 229888, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10940976; uint32_t buffer_offset
	}, ; 99: Microsoft.CSharp
	%struct.CompressedAssemblyDescriptor {
		i32 24576, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11170864; uint32_t buffer_offset
	}, ; 100: System.Collections.Concurrent
	%struct.CompressedAssemblyDescriptor {
		i32 22528, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11195440; uint32_t buffer_offset
	}, ; 101: System.Collections.Immutable
	%struct.CompressedAssemblyDescriptor {
		i32 15872, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11217968; uint32_t buffer_offset
	}, ; 102: System.Collections.NonGeneric
	%struct.CompressedAssemblyDescriptor {
		i32 11264, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11233840; uint32_t buffer_offset
	}, ; 103: System.Collections.Specialized
	%struct.CompressedAssemblyDescriptor {
		i32 31232, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11245104; uint32_t buffer_offset
	}, ; 104: System.Collections
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11276336; uint32_t buffer_offset
	}, ; 105: System.ComponentModel.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 95744, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11288112; uint32_t buffer_offset
	}, ; 106: System.ComponentModel.TypeConverter
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11383856; uint32_t buffer_offset
	}, ; 107: System.ComponentModel
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11389488; uint32_t buffer_offset
	}, ; 108: System.Console
	%struct.CompressedAssemblyDescriptor {
		i32 40448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11401776; uint32_t buffer_offset
	}, ; 109: System.Diagnostics.DiagnosticSource
	%struct.CompressedAssemblyDescriptor {
		i32 29184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11442224; uint32_t buffer_offset
	}, ; 110: System.Diagnostics.Process
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11471408; uint32_t buffer_offset
	}, ; 111: System.Diagnostics.TraceSource
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11483184; uint32_t buffer_offset
	}, ; 112: System.Drawing.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11494960; uint32_t buffer_offset
	}, ; 113: System.Drawing
	%struct.CompressedAssemblyDescriptor {
		i32 60416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11500080; uint32_t buffer_offset
	}, ; 114: System.Formats.Asn1
	%struct.CompressedAssemblyDescriptor {
		i32 22016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11560496; uint32_t buffer_offset
	}, ; 115: System.IO.Compression.Brotli
	%struct.CompressedAssemblyDescriptor {
		i32 32768, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11582512; uint32_t buffer_offset
	}, ; 116: System.IO.Compression
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11615280; uint32_t buffer_offset
	}, ; 117: System.IO.Pipelines
	%struct.CompressedAssemblyDescriptor {
		i32 416256, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11621424; uint32_t buffer_offset
	}, ; 118: System.Linq.Expressions
	%struct.CompressedAssemblyDescriptor {
		i32 55808, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12037680; uint32_t buffer_offset
	}, ; 119: System.Linq.Queryable
	%struct.CompressedAssemblyDescriptor {
		i32 150016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12093488; uint32_t buffer_offset
	}, ; 120: System.Linq
	%struct.CompressedAssemblyDescriptor {
		i32 14336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12243504; uint32_t buffer_offset
	}, ; 121: System.Memory
	%struct.CompressedAssemblyDescriptor {
		i32 124928, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12257840; uint32_t buffer_offset
	}, ; 122: System.Net.Http
	%struct.CompressedAssemblyDescriptor {
		i32 39424, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12382768; uint32_t buffer_offset
	}, ; 123: System.Net.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 7680, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12422192; uint32_t buffer_offset
	}, ; 124: System.Net.Requests
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12429872; uint32_t buffer_offset
	}, ; 125: System.Numerics.Vectors
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12434992; uint32_t buffer_offset
	}, ; 126: System.ObjectModel
	%struct.CompressedAssemblyDescriptor {
		i32 73216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12453424; uint32_t buffer_offset
	}, ; 127: System.Private.Uri
	%struct.CompressedAssemblyDescriptor {
		i32 397824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12526640; uint32_t buffer_offset
	}, ; 128: System.Private.Xml
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12924464; uint32_t buffer_offset
	}, ; 129: System.Runtime.InteropServices.RuntimeInformation
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12929584; uint32_t buffer_offset
	}, ; 130: System.Runtime.InteropServices
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12938800; uint32_t buffer_offset
	}, ; 131: System.Runtime.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 79360, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12943920; uint32_t buffer_offset
	}, ; 132: System.Runtime.Numerics
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13023280; uint32_t buffer_offset
	}, ; 133: System.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 124416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13038128; uint32_t buffer_offset
	}, ; 134: System.Security.Cryptography
	%struct.CompressedAssemblyDescriptor {
		i32 29696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13162544; uint32_t buffer_offset
	}, ; 135: System.Text.Encodings.Web
	%struct.CompressedAssemblyDescriptor {
		i32 378880, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13192240; uint32_t buffer_offset
	}, ; 136: System.Text.Json
	%struct.CompressedAssemblyDescriptor {
		i32 332288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13571120; uint32_t buffer_offset
	}, ; 137: System.Text.RegularExpressions
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13903408; uint32_t buffer_offset
	}, ; 138: System.Threading.Thread
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13908528; uint32_t buffer_offset
	}, ; 139: System.Threading
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13920816; uint32_t buffer_offset
	}, ; 140: System.Xml.ReaderWriter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13925936; uint32_t buffer_offset
	}, ; 141: System
	%struct.CompressedAssemblyDescriptor {
		i32 4608, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13931056; uint32_t buffer_offset
	}, ; 142: mscorlib
	%struct.CompressedAssemblyDescriptor {
		i32 7680, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13935664; uint32_t buffer_offset
	}, ; 143: netstandard
	%struct.CompressedAssemblyDescriptor {
		i32 2114560, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13943344; uint32_t buffer_offset
	}, ; 144: System.Private.CoreLib
	%struct.CompressedAssemblyDescriptor {
		i32 171008, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16057904; uint32_t buffer_offset
	}, ; 145: Java.Interop
	%struct.CompressedAssemblyDescriptor {
		i32 22328, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16228912; uint32_t buffer_offset
	}, ; 146: Mono.Android.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 2290688, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16251240; uint32_t buffer_offset
	} ; 147: Mono.Android
], align 4

@uncompressed_assemblies_data_size = dso_local local_unnamed_addr constant i32 18541928, align 4

@uncompressed_assemblies_data_buffer = dso_local local_unnamed_addr global [18541928 x i8] zeroinitializer, align 1

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/10.0.1xx @ db080b91dbe7947a69005a203ea98d9bb207ad04"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
