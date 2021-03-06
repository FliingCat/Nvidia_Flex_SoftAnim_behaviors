﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
//using NVIDIA.Flex;
using uFlex;


// TODO: the MLAgents.Academy class is now a singleton that cannot be subclassed, so this code is outdated.
// See here for how it should be designed now:
// https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Learning-Environment-Design.md
// Most of the code here should probably move into the FlexAgent class where appropriate or be moved into
// an EnvironmentReset method that can be registered with Academy.Instance.OnEnvironmentReset.

public class FlexAcademy {   // : Academy {
	/// <summary>
	/// Maximum number of particles in scene.
	/// </summary>
	public int maxParticles;

	/// <summary>
	/// Radius of flex particles.
	/// </summary>
	public float particleRadius = 0.5f;

	/// <summary>
	/// Dimension of particle state (pos_x, pos_y, pos_z, mass, vel_x, vel_y, vel_z, id).
	/// </summary>
	const int particleDimension = 8;

	/// <summary>
	/// The flex container or solver.
	/// </summary>
	public FlexContainer flexContainer;

	/// <summary>
	/// The flex agent.
	/// </summary>
	public FlexAgent agent;

	/// <summary>
	/// The target the agent has to push.
	/// </summary>
	public GameObject target;

    /// <summary>
    /// Initantiates and initializes a new flex container.
    /// </summary>
    /// <param name="maxParticles">Maximum number of particles of flex container.</param>
    /// <param name="particleRadius">Radius of flex particles.</param>
    private void Start()
    {
        maxParticles = flexContainer.m_activeParticlesCount;
        //print(maxParticles);
    }

    void InitializeFlexContainer(int maxParticles, float particleRadius)
	{
        //flexContainer = ScriptableObject.CreateInstance<FlexContainer>();
        // number of particles of all FleX objects, in this case 8 agent and 8 target particles
        maxParticles = flexContainer.m_activeParticlesCount ;
		//flexContainer.radius = particleRadius;
		//flexContainer.solidRest = particleRadius;
		//flexContainer.collisionDistance = particleRadius / 2.0f;
	}

	/// <summary>
	/// Checks if application is runnning in headless mode.
	/// </summary>
	/// <returns><c>true</c>, if headless mode is used, <c>false</c> otherwise.</returns>
	public static bool isHeadlessMode()
	{
		return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
	}

	/// <summary>
	/// Adds a flex solid actor to gameObject.
	/// </summary>
	/// <param name="gameObject">Input gameObject with mesh to which flex solid actor should be attached.</param>
	//void AddFlexSoftActor(GameObject gameObject)
	//{
	//	// Build the solid flex asset
	//	FlexSolidAsset flexSolidAsset = ScriptableObject.CreateInstance<FlexSolidAsset> ();
	//	flexSolidAsset.particleSpacing = particleRadius;
	//	flexSolidAsset.boundaryMesh = gameObject.GetComponent<MeshFilter> ().mesh;
	//	flexSolidAsset.Rebuild ();
	//	// Build the solid flex actor
	//	FlexSolidActor flexActor = gameObject.AddComponent<FlexSolidActor> ();
	//	flexActor.asset = flexSolidAsset;
	//	flexActor.container = this.flexContainer;
	//	if(!isHeadlessMode())
	//		flexActor.drawParticles = true;
	//	flexActor.enabled = false;
	//	flexActor.enabled = true;
	//}

	/// <summary>
	/// Returns a new random float between -4 to -1, and 1 to 4.
	/// </summary>
	//float NewRandomPosition()
	//{
	//	float x = Random.value * 8.0f - 4.0f;
	//	while (x < 1.0f && x > -1.0f)
	//		x = Random.value * 8.0f - 4.0f;
	//	return x;
	//}

	/// <summary>
	/// Initializes target cube and adds flex actor.
	/// </summary>
	//void InitializeTarget()
	//{
	//	target = GameObject.CreatePrimitive (PrimitiveType.Cube);
	//	target.name = "Target";
	//	target.GetComponent<MeshRenderer>().material = (Material) Resources.Load ("Materials/Blue", typeof(Material));
	//	target.transform.position =  new Vector3(NewRandomPosition(), 0.5f, NewRandomPosition());
	//	AddFlexSolidActor (target);
	//}

	/// <summary>
	/// Initializes agent cube and adds flex actor.
	/// </summary>
	//void InitializeAgent()
	//{
	//	agent.GetComponent<MeshRenderer>().material = (Material) Resources.Load ("Materials/Red", typeof(Material));
	//	AddFlexSolidActor (agent.gameObject);
	//}
		

	/// <summary>
	/// Sets the observation size to the maximum number of particles that the flex container can hold.
	/// </summary>
	void SetObservationVectorSize()
	{
        //print(maxParticles);
//		agent.brain.brainParameters.vectorObservationSize = maxParticles/* * particleDimension*/;
	}

	/// <summary>
	/// Initializes academy by initializing a new flex container, target and agent.
	/// </summary>
	public void InitializeAcademy()
	{
		InitializeFlexContainer(maxParticles, particleRadius);
		//InitializeTarget();
		//InitializeAgent();
		SetObservationVectorSize();
	}

	/// <summary>
	/// Resets the academy by teleporting the target to a new random location and resetting the number of steps.
	/// </summary>
	public void AcademyReset()
	{
		//target.GetComponent<FlexActor>().Teleport
		//(new Vector3(NewRandomPosition(), 0.5f, NewRandomPosition()),
		//	Quaternion.Euler(Vector3.zero));
	}
}
