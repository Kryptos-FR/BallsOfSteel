// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Panels;

namespace BallsOfSteel.UI
{
    public class ControllerScript : SyncScript
    {
        public UIComponent UI { get; set; }

        private Thumb leftThumb;
        private Thumb rightThumb;
        private Vector2 mousePosition;

        public override void Start()
        {
            var deviceWidth = GraphicsDevice.Presenter.Description.BackBufferWidth;
            var deviceHeight = GraphicsDevice.Presenter.Description.BackBufferHeight;
            var deviceSize = new Vector2(deviceWidth, deviceHeight);

            UI = UI ?? Entity.Get<UIComponent>();

            var page = UI.Page;
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
                    switch (pointerEvent.State)
                    {
                        case PointerState.Down:
                            // TODO
                            break;
                        case PointerState.Move:
                            var position = pointerEvent.Position;
                            if (position.X < 0.5f)
                                leftThumb.Move(pointerEvent.Position);
                            else
                                rightThumb.Move(pointerEvent.Position);
                            // TODO
                            break;
                        case PointerState.Up:
                            // TODO
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


    }

    internal class Thumb
    {
        private readonly Grid parentGrid;
        private readonly Vector3 parentCenter;
        private readonly UIElement thumbControl;
        private readonly Vector3 thumbCenter;
        private readonly Vector2 deviceSize;
        private readonly Vector3 uiResolution;

        private bool isPressed;

        public Thumb([NotNull] UIElement thumbControl, Vector2 deviceSize, Vector3 uiResolution)
        {
            this.deviceSize = deviceSize;
            this.uiResolution = uiResolution;

            this.thumbControl = thumbControl;
            thumbCenter = new Vector3(thumbControl.Width, thumbControl.Height, 0) / 2;
            parentGrid = (Grid)thumbControl.Parent;
            parentCenter = new Vector3(parentGrid.Width, parentGrid.Height, 0)/2;
        }

        public void Move(Vector2 screenPosition)
        {
            var uiPosition = uiResolution * new Vector3(screenPosition, 0);
            var parentPosition = parentGrid.WorldMatrix.TranslationVector + uiResolution/2;
            if (uiPosition.X < parentPosition.X - parentCenter.X || uiPosition.Y < parentPosition.Y - parentCenter.Y ||
                uiPosition.X > parentPosition.X + parentCenter.X || uiPosition.Y > parentPosition.Y + parentCenter.Y)
                return;

            var absolutePosition = uiPosition - parentPosition + parentCenter - thumbCenter;
            var margin = thumbControl.Margin;
            margin.Left = absolutePosition.X;
            margin.Top = absolutePosition.Y;
            thumbControl.Margin = margin;
        }
    }
}
