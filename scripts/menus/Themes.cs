using Godot;
using System;

namespace ThemeManager
{
    public partial class Themes : Node
    {
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
        public static Theme CreateMenuButtonTheme()
        {
            Theme theme = new Theme();
            theme.SetColor("font_color", "Button", new Color(0.0f, 0.0f, 0.0f, 1f));
            Font customFont = ResourceLoader.Load<FontFile>("res://resources/menuThemes/black-and-white-picture-cyrillic.ttf");
            theme.SetFont("font", "Label", customFont);
            theme.SetFontSize("font_size", "Label", 40);
            theme.SetFont("font", "Button", customFont);
            theme.SetFontSize("font_size", "Button", 40);

            var normalButtonStyle = new StyleBoxFlat();
            normalButtonStyle.BgColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            normalButtonStyle.CornerRadiusTopLeft = 5;
            normalButtonStyle.CornerRadiusTopRight = 5;
            normalButtonStyle.CornerRadiusBottomLeft = 5;
            normalButtonStyle.CornerRadiusBottomRight = 5;
            normalButtonStyle.ContentMarginLeft = 10; // Отступ слева для текста/иконки
            normalButtonStyle.ContentMarginRight = 10;
            normalButtonStyle.ContentMarginTop = 5;
            normalButtonStyle.ContentMarginBottom = 5;
            theme.SetStylebox("normal", "Button", normalButtonStyle);

            // Стиль для кнопки при наведении
            var hoverButtonStyle = new StyleBoxFlat();
            hoverButtonStyle.BgColor = new Color((238 / 255), (209 / 255), (141 / 255), 0.4f);
            hoverButtonStyle.SetCornerRadiusAll(5); // Установить все углы сразу
            hoverButtonStyle.ContentMarginLeft = 10;
            hoverButtonStyle.ContentMarginRight = 10;
            hoverButtonStyle.ContentMarginTop = 5;
            hoverButtonStyle.ContentMarginBottom = 5;
            theme.SetStylebox("hover", "Button", hoverButtonStyle);

            var labelStyle = new StyleBoxFlat();
            //theme.SetStylebox("");

            return theme;
        }
    }
}
