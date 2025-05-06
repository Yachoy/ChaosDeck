using Godot;
using System;

public partial class ParamatersTwoPlayers : Panel
{
	[Export]
	private Label player1_hp;
	[Export]
	private Label player1_mana;
	[Export]
	private Label player1_power_mana;

	[Export]
	private Label player2_hp;
	[Export]
	private Label player2_mana;
	[Export]
	private Label player2_power_mana;

	private CardController cc;
	public void set_hp(bool is_player_1, int value){
		if(is_player_1){
			player1_hp.Text = value.ToString();
		}else{
			player1_hp.Text = value.ToString();
		}
	}
	public void set_mana(bool is_player_1, int value){
		if(is_player_1){
			player1_mana.Text = value.ToString();
		}else{
			player1_mana.Text = value.ToString();
		}
	}
	public void set_power_mana(bool is_player_1, int value){
		if(is_player_1){
			player1_power_mana.Text = value.ToString();
		}else{
			player1_power_mana.Text = value.ToString();
		}
	}
}
