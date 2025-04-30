// Файл: outline_3d.gdshader
shader_type spatial;
render_mode cull_front, unshaded;

uniform vec4 outline_color : source_color = vec4(1.0, 0.75, 0.0, 1.0);
// Сделайте максимальное значение немного больше, чтобы легче было тестировать Z-fighting
uniform float outline_width : hint_range(0.0, 1.0, 0.001) = 0.01;

void vertex() {
	// Просто смещаем вершину вдоль нормали на фиксированную величину в мировом пространстве
	VERTEX += NORMAL * outline_width;
}

void fragment() {
	ALBEDO = outline_color.rgb;
	ALPHA = outline_color.a;

}