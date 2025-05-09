using Godot;
using System;

public partial class CardUI : Node2D
{

	[Export] 
	private Sprite2D target;
	[Export]
	private float padding = 0.1f; // Процент отступа от каждой стороны (0.1 = 10%)

	[Export]
	public Label mana;
	[Export]
	public Label type;
	[Export]
	public Label damage;
	[Export]
	public Label hp;
	[Export]
	public Control Control;
	public string description;

	public override void _Ready(){
		//ChangeTexture("res://resources/CardsStorage/images/dwarf.png");
	}

	public void ChangeTexture(string newTexturePath)
	{
		var newTexture = GD.Load<Texture2D>(newTexturePath);

		if (newTexture != null)
		{
			target.Texture = newTexture;
            AdjustSpriteToTargetObject(); // Вызываем после смены текстуры
		}
		else
		{
			GD.PrintErr($"Failed to load texture from path: {newTexturePath}");
		}
	}

	private void AdjustSpriteToTargetObject()
	{
		if (target == null || target.Texture == null)
		{
			GD.PrintErr("Target Sprite2D or its Texture is not set.");
			return;
		}

		// Вариант 1: Предполагаем, что CardUI имеет размеры (например, если это Control узел или его размер задан вручную)
		// Если CardUI является Control узлом, вы можете использовать GetRect().Size.
		// Если CardUI просто Node2D, его "размер" не является встроенным свойством.

		// Вариант 2: Vec(100x150) единиц.
		Vector2 cardLogicalSize = new Vector2(132, 131); // Замените на реальные логические размеры вашей карты

		// Применяем отступ к логическим размерам карты
		float effectiveWidth = cardLogicalSize.X * (1.0f - 2.0f * padding);
		float effectiveHeight = cardLogicalSize.Y * (1.0f - 2.0f * padding);

		// Получаем размеры текстуры в пикселях
		Vector2 textureSize = target.Texture.GetSize();

		// Избегаем деления на ноль
		if (textureSize.X <= 0 || textureSize.Y <= 0)
		{
			GD.PrintErr("Texture has invalid dimensions.");
			return;
		}

		// Рассчитываем пропорции текстуры
		float textureAspectRatio = textureSize.X / textureSize.Y;

		// Рассчитываем пропорции эффективной области
		float effectiveAspectRatio = effectiveWidth / effectiveHeight;

		// Определяем масштабный коэффициент
		float scaleFactor;
		if (textureAspectRatio > effectiveAspectRatio)
		{
			// Текстура шире относительно своей высоты, чем эффективная область.
			// Масштабируем так, чтобы ширина текстуры соответствовала эффективной ширине.
			// EffectiveWidth = TextureWidth * Scale.X (предполагая, что Scale.X = Scale.Y для сохранения пропорций)
			// Scale.X = EffectiveWidth / TextureWidth
			scaleFactor = effectiveWidth / textureSize.X;
		}
		else
		{
			// Текстура выше относительно своей ширины, чем эффективная область.
			// Масштабируем так, чтобы высота текстуры соответствовала эффективной высоте.
			// EffectiveHeight = TextureHeight * Scale.Y (предполагая, что Scale.X = Scale.Y)
			// Scale.Y = EffectiveHeight / TextureHeight
			scaleFactor = effectiveHeight / textureSize.Y;
		}

		// Применяем вычисленный масштаб к Sprite2D
		target.Scale = new Vector2(scaleFactor, scaleFactor);

		// Опционально: Центрируем спрайт внутри CardUI
		// Если CardUI имеет размер, то центр будет (CardLogicalSize.X / 2, CardLogicalSize.Y / 2)
		// По умолчанию, Sprite2D отрисовывается относительно своего центра.
		// Если центр спрайта не в (0,0), возможно, вам потребуется настроить Position.
		// Если Origin спрайта в центре, то его Position должен быть в центре целевой области.
		// Если CardUI имеет логический размер и центр в (0,0), Position спрайта должен быть (0,0).
		target.Position = Vector2.Zero; // Если центр CardUI в (0,0) и Origin спрайта в центре
	}
}
