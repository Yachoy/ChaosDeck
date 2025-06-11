using Godot;
using System;

public partial class CardUI : Control
{

	[Export] 
	public TextureRect target;
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

	public string nameCard;

	public event Action<CardUI, MouseButton> MouseRLClick;

	public override void _Ready()
	{
		//ChangeTexture("res://resources/CardsStorage/images/dwarf.png");
		target.GuiInput += Input;
	}

	public void ChangeTexture(string newTexturePath)
	{
		var newTexture = GD.Load<Texture2D>(newTexturePath);
        ChangeTexture(newTexture);
	}

	public void Input(InputEvent @event)
    {
        // 1. Проверяем, является ли событие кликом мыши
		if (@event is InputEventMouseButton mouseButtonEvent)
		{

			if (mouseButtonEvent.Pressed)
			{
				MouseRLClick?.Invoke(this, mouseButtonEvent.ButtonIndex);
			}
		}
    }

    public void ChangeTexture(Texture2D texture)
	{
		target.Texture = texture;
        AdjustSpriteToTargetObject();
	}

	private void AdjustSpriteToTargetObject()
	{
		if (target == null || target.Texture == null)
		{
			GD.PrintErr("Target Sprite2D or its Texture is not set.");
			return;
		}

		Vector2 cardLogicalSize = new Vector2(132, 131); // Замените на реальные логические размеры вашей карты

		float effectiveWidth = cardLogicalSize.X * (1.0f - 2.0f * padding);
		float effectiveHeight = cardLogicalSize.Y * (1.0f - 2.0f * padding);

		Vector2 textureSize = target.Texture.GetSize();

		if (textureSize.X <= 0 || textureSize.Y <= 0)
		{
			GD.PrintErr("Texture has invalid dimensions.");
			return;
		}

		float textureAspectRatio = textureSize.X / textureSize.Y;
		float effectiveAspectRatio = effectiveWidth / effectiveHeight;
		float scaleFactor;
		if (textureAspectRatio > effectiveAspectRatio)
		{
			scaleFactor = effectiveWidth / textureSize.X;
		}
		else
		{
			scaleFactor = effectiveHeight / textureSize.Y;
		}

		// Применяем вычисленный масштаб к Sprite2D
		target.Scale = new Vector2(scaleFactor, scaleFactor);
		target.Position = Vector2.Zero; // Если центр CardUI в (0,0) и Origin спрайта в центре
	}
}
