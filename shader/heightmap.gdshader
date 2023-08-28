shader_type spatial;

varying highp vec2 divisions;
varying highp float height;

uniform highp int chunkCount;
uniform highp vec2 chunkPos;
uniform highp float heightScale;

uniform highp sampler2D heightMap;
uniform highp sampler2D normalMap;

void vertex() {
  divisions.x = 1.0 / float(chunkCount);
  divisions.y = 1.0 / float(chunkCount);
  height = texture(heightMap, (UV.xy*divisions) + (chunkPos*divisions)).r * heightScale;
  VERTEX = VERTEX.xyz + vec3(0,height,0);
}

void fragment() {
  ALBEDO = vec3(height / heightScale);
  RIM = 0.2;
  METALLIC = 0.0;
  ROUGHNESS = 0.01 * (1.0);
  
  NORMAL_MAP = texture(normalMap, (UV.xy*divisions) + (chunkPos*divisions)).xyz;
}
