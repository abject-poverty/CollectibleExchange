#version 330 core
//#include dither.fsh

precision highp float;

in vec2 uv;
out vec4 outColor;

uniform sampler2D iDepthBuffer;
uniform sampler2D iColor;
uniform sampler2D iLight;

uniform vec2 iResolution;
uniform vec2 iMouse;
uniform vec2 iCamera;
uniform vec3 iSunPos;
uniform vec3 iMoonPos;
uniform vec3 iPlayerPosition;
uniform vec3 iLookBlockPos;
uniform vec3 iLookEntityPos;
uniform vec3 iCameraPos;

//0 or 1
uniform vec4 iControls1; //Backward, Down, FloorSitting, Forward
uniform vec4 iControls2; //Jump, Left, LeftMouseDown, Right
uniform vec4 iControls3; //RightMouseDown, Sitting, Sneak, Sprint
uniform vec2 iControls4; //TriesToMove, Up

//some float or other
uniform float iMoonPhase;
uniform float iTime;
uniform float iTemperature;
uniform float iRainfall;
uniform float iCurrentHealth;
uniform float iMaxHealth;

//scalar
uniform float iTempScalar;

//By ID, if it's null, it'll be -1
uniform float iActiveItem;
uniform float iLookingAtBlock;
uniform float iLookingAtEntity;

vec3 red = vec3(1, 0, 0);
vec3 yellow = vec3(1, 1, 0);
vec3 blue = vec3(0, 0, 1);

//Current Enum Tool Type
//NoTool: -1, Knife: 0, Pickaxe: 1, Axe: 2, Sword: 3, Shovel: 4, Hammer: 5, Spear: 6, Bow: 7, Shears: 8, Sickle: 9, Hoe: 10, Saw: 11, Chisel: 12,
uniform float iActiveTool;

float Circle(vec2 uv2, vec2 p, float r, float blur){
	return smoothstep(r, r - blur, length(uv2 - p));
}

vec2 Rotate(float speedx, float speedy, float radius){
	return vec2(sin(iTime*speedx)*radius, cos(iTime*speedy)*radius);
}

float near = 0.001; 
float far  = 1500.0;

float Depth() 
{
	float depth = texture(iDepthBuffer, uv).x;
    float z = depth * 2.0 - 1.0;
    return (2.0 * near * far) / (far + near - z * (far - near));// + NoiseFromPixelPosition(ivec2(int(iResolution.x * uv.x), int(iResolution.y * uv.y)), int(iTime), int(iResolution.x)).x;
	//return depth;
}
float depth = Depth();

vec3 vDepth = vec3(Depth());
vec4 Color = texture(iColor, uv);
vec4 Light = texture(iLight, uv);
float iGlow = Light.r;
float iGodRay = Light.g;

vec4 pColor(vec2 uva){
	return texture(iColor, uva);
}

vec3 camVec = vec3(cos(iCamera.x) * cos(iCamera.y), sin(iCamera.x), cos(iCamera.x) * sin(-iCamera.y));
vec3 camNrm = normalize(camVec);

//vec3 Nrm = normalize(vec3(Depth(), 1.0 - Depth(), 1.0));
float rng = fract(sin(dot(vec2(uv.x, uv.y), vec2(12.9898, 78.233)))*43758.5453123);
const vec3 nrmavg = vec3(128.0/255.0, 128.0/255.0, 1);

vec3 test()
{
	vec4 pos = vec4((uv.xy * 0.5) / (depth), uv.x, 1.0) * uv.y;
	vec3 n = normalize(cross(dFdx(pos.xyz), dFdy(pos.xyz))) * 0.5 + 0.5;
	return n;
}
vec3 Nrm = test();

void main () 
{
	vec2 uv2 = uv - 0.5;
	uv2.x *= iResolution.x / iResolution.y;

	//outColor = vec4(clamp(Color.xyz / Nrm.y, 0, 1), 1.0);
}

