float distanceFromPoint(float3 referencePoint, float3 evaluatingPoint)
{
	float distance = length(referencePoint - evaluatingPoint);
	return distance;
}

//discard drawing of a point in the world if it is outside of a defined circle
void circleClip(float3 posWorld, float3 posCircle, float radiusCircle) {

	posCircle.y = 0;
	posWorld.y = 0;

	float dist = distanceFromPoint(posWorld, posCircle);
	clip( (radiusCircle - dist) );

}
