﻿using System;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiButton : GuiControl, IGuiButton
	{

        public string Text
        {
            get => TextElement.Text;
	        set => TextElement.Text = value;
        }
	    public string TranslationKey
	    {
		    get => TextElement.TranslationKey;
		    set => TextElement.TranslationKey = value;
	    }

        protected GuiTextElement TextElement { get; }
        protected Action Action { get; }
		
	    public GuiButton(Action action = null) : this(string.Empty, action)
	    {
			
	    }
		
        public GuiButton(string text, Action action = null)
        {
            Background	  = GuiTextures.ButtonDefault;
	        DisabledBackground	  = GuiTextures.ButtonDisabled;
            HighlightedBackground = GuiTextures.ButtonHover;
            FocusedBackground	  = GuiTextures.ButtonFocused;

			Background.RepeatMode	 = TextureRepeatMode.NoScaleCenterSlice;
	        DisabledBackground.RepeatMode	 = TextureRepeatMode.NoScaleCenterSlice;
	        HighlightedBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
	        FocusedBackground.RepeatMode	 = TextureRepeatMode.NoScaleCenterSlice;
			
            Action = action;
            MinHeight = 20;
	        MinWidth = 20;

	        MaxHeight = 22;
	        MaxWidth = 200;
			Padding = new Thickness(5, 5);
			Margin = new Thickness(2);

	        AddChild(TextElement = new GuiTextElement()
            {
				Margin =  Thickness.Zero,
                Anchor = Alignment.MiddleCenter,
                Text = text,
                TextColor = TextColor.White,
				TextOpacity = 0.875f,
				FontStyle = FontStyle.DropShadow
            });

	        Modern = true;

        }

	    private bool _isModern = false;
	    public bool Modern
	    {
		    get { return _isModern; }
		    set
		    {
			    if (value)
			    {
				    _isModern = true;

				    Background = Color.Transparent;// new Color(Color.Black * 0.25f, 0.25f) ;
				    DisabledBackground = Color.Transparent;
				    FocusedBackground = Color.TransparentBlack;

				    HighlightedBackground = new Color(Color.Black * 0.8f, 0.5f);

				}
			    else
			    {
				    _isModern = false;
				    Background = GuiTextures.ButtonDefault;
				    DisabledBackground = GuiTextures.ButtonDisabled;
				    HighlightedBackground = GuiTextures.ButtonHover;
				    FocusedBackground = GuiTextures.ButtonFocused;
				}
			}
	    }

	    protected override void OnHighlightActivate()
	    {
		    base.OnHighlightActivate();
		    if (_isModern)
		    {
			    TextElement.TextColor = TextColor.Cyan;
			}
		    else
		    {
			    TextElement.TextColor = TextColor.Yellow;
			}
		}

	    protected override void OnHighlightDeactivate()
	    {
		    base.OnHighlightDeactivate();

			TextElement.TextColor = TextColor.White;
	    }

	    protected override void OnCursorPressed(Point cursorPosition)
		{
			Focus();
		}

		protected override void OnFocusActivate()
		{
			Action?.Invoke();
		}


		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	    {
		    base.OnDraw(graphics, gameTime);
	    }

	    protected override void OnEnabledChanged()
	    {
		    if (_isModern)
		    {
			    if (!Enabled)
			    {
				    TextElement.TextColor = TextColor.DarkGray;
				    // TextElement.TextOpacity = 0.3f;
			    }
			    else
			    {
				    TextElement.TextColor = TextColor.White;
				    TextElement.TextOpacity = 1f;
			    }
		    }
	    }
    }
}