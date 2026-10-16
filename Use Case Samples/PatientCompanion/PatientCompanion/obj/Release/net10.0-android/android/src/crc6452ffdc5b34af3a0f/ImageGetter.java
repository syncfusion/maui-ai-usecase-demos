package crc6452ffdc5b34af3a0f;


public class ImageGetter
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.text.Html.ImageGetter
{

	public ImageGetter ()
	{
		super ();
		if (getClass () == ImageGetter.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.Platform.ImageGetter, Microsoft.Maui", "", this, new java.lang.Object[] {  });
		}
	}

	public android.graphics.drawable.Drawable getDrawable (java.lang.String p0)
	{
		return n_getDrawable (p0);
	}

	private native android.graphics.drawable.Drawable n_getDrawable (java.lang.String p0);

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
