using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Events;
using SiliconStudio.Xenko.UI.Panels;

namespace BallsOfSteel.UI
{
    public class ControllerScript : SyncScript
    {
        public UIComponent UI { get; set; }

        private Thumb leftThumb;
        private Thumb rightThumb;
        private Button attackButton;
        private Button jumpButton;

        public override void Start()
        {
            var deviceWidth = GraphicsDevice.Presenter.Description.BackBufferWidth;
            var deviceHeight = GraphicsDevice.Presenter.Description.BackBufferHeight;
            var deviceSize = new Vector2(deviceWidth, deviceHeight);

            UI = UI ?? Entity.Get<UIComponent>();

            var page = UI.Page;
            attackButton = page.RootElement.FindVisualChildOfType<Button>("AttackButton");
            jumpButton = page.RootElement.FindVisualChildOfType<Button>("JumpButton");
            leftThumb = new Thumb(page.RootElement.FindVisualChildOfType<UIElement>("Lthumb"), deviceSize, UI.Resolution);
            rightThumb = new Thumb(page.RootElement.FindVisualChildOfType<UIElement>("Rthumb"), deviceSize, UI.Resolution);
        }

        /// <inheritdoc />
        public override void Update()
        {
            // Pointer
            if (Input.HasPointer)
            {
                foreach (var pointerEvent in Input.PointerEvents)
                {
                    var position = pointerEvent.Position;
                    switch (pointerEvent.State)
                    {
                        case PointerState.Down:
                            if (position.X < 0.5f)
                                leftThumb.Down(position);
                            else
                                rightThumb.Down(position);
                            break;

                        case PointerState.Move:
                            if (position.X < 0.5f)
                                leftThumb.Move(position);
                            else
                                rightThumb.Move(position);
                            break;

                        case PointerState.Up:
                            if (position.X < 0.5f)
                                leftThumb.Up(position);
                            else
                                rightThumb.Up(position);
                            break;

                        case PointerState.Out:
                        case PointerState.Cancel:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void Cancel()
        {
            if (attackButton != null)
                attackButton.Click -= OnAttackButonClicked;
            if (jumpButton != null)
                jumpButton.Click -= OnJumpButonClicked;
        }

        private void OnAttackButonClicked(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void OnJumpButonClicked(object sender, RoutedEventArgs e)
        {
            // TODO
        }


    }

    internal class Thumb
    {
        private readonly Grid parentGrid;
        private readonly Vector2 parentHalfSize;
        private readonly UIElement thumbControl;
        private readonly Vector2 thumbCenter;
        private readonly Vector2 deviceSize;
        private readonly Vector2 uiResolution;

        private bool isDown;

        public Thumb([NotNull] UIElement thumbControl, Vector2 deviceSize, Vector3 uiResolution)
        {
            this.deviceSize = deviceSize;
            this.uiResolution = uiResolution.XY();

            this.thumbControl = thumbControl;
            thumbCenter = new Vector2(thumbControl.Width, thumbControl.Height) / 2;
            parentGrid = (Grid)thumbControl.Parent;
            parentHalfSize = new Vector2(parentGrid.Width, parentGrid.Height)/2;
        }

        public void Down(Vector2 screenPosition)
        {
            var uiPosition = uiResolution * screenPosition;
            var parentPosition = parentGrid.WorldMatrix.TranslationVector.XY() + uiResolution / 2;

            if ((uiPosition - parentPosition).LengthSquared() > parentHalfSize.X * parentHalfSize.X)
                return;

            isDown = true;
            Move(screenPosition);
        }

        public void Move(Vector2 screenPosition)
        {
            if (!isDown)
                return;

            var uiPosition = uiResolution * screenPosition;
            var parentPosition = parentGrid.WorldMatrix.TranslationVector.XY() + uiResolution/2;
            var offset = uiPosition - parentPosition;
            if (offset.LengthSquared() > parentHalfSize.X * parentHalfSize.X)
            {
                // Project on the outside circle
                var direction = Vector2.Normalize(uiPosition - parentPosition);
                offset = direction * parentHalfSize.X;
            }

            var absolutePosition = offset + parentHalfSize - thumbCenter;
            var margin = thumbControl.Margin;
            margin.Left = absolutePosition.X;
            margin.Top = absolutePosition.Y;
            thumbControl.Margin = margin;
        }

        public void Up(Vector2 screenPosition)
        {
            if (!isDown)
                return;

            isDown = false;
            var absolutePosition = parentHalfSize - thumbCenter;
            var margin = thumbControl.Margin;
            margin.Left = absolutePosition.X;
            margin.Top = absolutePosition.Y;
            thumbControl.Margin = margin;
        }
    }
}
