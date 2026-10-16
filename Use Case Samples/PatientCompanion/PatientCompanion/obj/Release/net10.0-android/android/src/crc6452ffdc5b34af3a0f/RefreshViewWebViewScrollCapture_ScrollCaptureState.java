package crc6452ffdc5b34af3a0f;


public class RefreshViewWebViewScrollCapture_ScrollCaptureState
	extends com.microsoft.maui.RefreshViewJavaScriptInterface
	implements
		mono.android.IGCUserPeer
{

	public RefreshViewWebViewScrollCapture_ScrollCaptureState ()
	{
		super ();
		if (getClass () == RefreshViewWebViewScrollCapture_ScrollCaptureState.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.Platform.RefreshViewWebViewScrollCapture+ScrollCaptureState, Microsoft.Maui", "", this, new java.lang.Object[] {  });
		}
	}

@android.webkit.JavascriptInterface
	public void setCanScrollUp (boolean p0)
	{
		n_setCanScrollUp (p0);
	}

	private native void n_setCanScrollUp (boolean p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
