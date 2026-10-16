package crc6452ffdc5b34af3a0f;


public class ClearButtonClickListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.View.OnClickListener
{

	public ClearButtonClickListener ()
	{
		super ();
		if (getClass () == ClearButtonClickListener.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.Platform.ClearButtonClickListener, Microsoft.Maui", "", this, new java.lang.Object[] {  });
		}
	}

	public ClearButtonClickListener (crc6452ffdc5b34af3a0f.MauiMaterialSearchBarTextInputLayout p0)
	{
		super ();
		if (getClass () == ClearButtonClickListener.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.Platform.ClearButtonClickListener, Microsoft.Maui", "Microsoft.Maui.Platform.MauiMaterialSearchBarTextInputLayout, Microsoft.Maui", this, new java.lang.Object[] { p0 });
		}
	}

	public void onClick (android.view.View p0)
	{
		n_onClick (p0);
	}

	private native void n_onClick (android.view.View p0);

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
