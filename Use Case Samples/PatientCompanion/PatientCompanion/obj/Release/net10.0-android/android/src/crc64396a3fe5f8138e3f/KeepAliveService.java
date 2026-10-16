package crc64396a3fe5f8138e3f;


public class KeepAliveService
	extends android.app.Service
	implements
		mono.android.IGCUserPeer
{

	public KeepAliveService ()
	{
		super ();
		if (getClass () == KeepAliveService.class) {
			mono.android.TypeManager.Activate ("AndroidX.Browser.CustomTabs.KeepAliveService, Xamarin.AndroidX.Browser", "", this, new java.lang.Object[] {  });
		}
	}

	public android.os.IBinder onBind (android.content.Intent p0)
	{
		return n_onBind (p0);
	}

	private native android.os.IBinder n_onBind (android.content.Intent p0);

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
