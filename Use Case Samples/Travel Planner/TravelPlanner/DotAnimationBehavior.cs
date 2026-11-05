using Microsoft.Maui.Controls.Shapes;
using System.Threading.Tasks;

namespace TravelPlanner
{
    class DotAnimationBehavior : Behavior<HorizontalStackLayout>
    {
        private Ellipse? dot1, dot2, dot3, dot4;
        private bool isAnimating = false;
        private bool animationRunning = false;
        private readonly double animationDuration = 120;
        private Color DotDefaultColor = Color.FromArgb("#d0af76");
        private Color AnimationColor = Color.FromArgb("#73623c");

        protected override void OnAttachedTo(HorizontalStackLayout bindable)
        {
            base.OnAttachedTo(bindable);

            // Initialize dots by finding them by their names
            dot1 = bindable.FindByName<Ellipse>("dot1");
            dot2 = bindable.FindByName<Ellipse>("dot2");
            dot3 = bindable.FindByName<Ellipse>("dot3");
            dot4 = bindable.FindByName<Ellipse>("dot4");

            // start animation and mark running flag
            isAnimating = true;
            this.StartAnimation();
        }

        protected override void OnDetachingFrom(HorizontalStackLayout bindable)
        {
            // Stop animation and clear dot references
            isAnimating = false;
            // Let any running animation observe the flag and exit, then clear refs
            dot1 = dot2 = dot3 = dot4 = null;
            base.OnDetachingFrom(bindable);
        }

        private async void StartAnimation()
        {
            if (animationRunning)
                return;

            animationRunning = true;
            try
            {
                // Continue while flagged and while any dot is still in the visual tree
                while (isAnimating && (dot1?.Parent != null || dot2?.Parent != null || dot3?.Parent != null || dot4?.Parent != null))
                {
                    if (!isAnimating) break;
                    if (dot1 != null)
                    {
                        await AnimateDot(dot1);
                    }
                    if (!isAnimating) break;
                    if (dot2 != null)
                    {
                        await AnimateDot(dot2);
                    }
                    if (!isAnimating) break;
                    if (dot3 != null)
                    {
                        await AnimateDot(dot3);
                    }
                    if (!isAnimating) break;
                    if (dot4 != null)
                    {
                        await AnimateDot(dot4);
                    }
                }
            }
            finally
            {
                animationRunning = false;
                isAnimating = false;
            }
        }

        private async Task AnimateDot(Ellipse dot)
        {
            // Perform animation on the dot
            if (!isAnimating) return;
            dot.Fill = new SolidColorBrush(AnimationColor);
            await dot.TranslateTo(0, -2, (uint)animationDuration);
            if (!isAnimating) return;
            await dot.TranslateTo(0, 0, (uint)animationDuration);
            if (!isAnimating) return;
            dot.Fill = new SolidColorBrush(DotDefaultColor);
        }
    }
}
