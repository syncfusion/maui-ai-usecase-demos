package mono.com.google.android.material.textfield;


public class TextInputLayout_OnEditTextAttachedListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.google.android.material.textfield.TextInputLayout.OnEditTextAttachedListener
{

	public TextInputLayout_OnEditTextAttachedListenerImplementor ()
	{
		super ();
		if (getClass () == TextInputLayout_OnEditTextAttachedListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Google.Android.Material.TextField.TextInputLayout+IOnEditTextAttachedListenerImplementor, Xamarin.Google.Android.Material", "", this, new java.lang.Object[] {  });
		}
	}

	public void onEditTextAttached (com.google.android.material.textfield.TextInputLayout p0)
	{
		n_onEditTextAttached (p0);
	}

	private native void n_onEditTextAttached (com.google.android.material.textfield.TextInputLayout p0);

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
