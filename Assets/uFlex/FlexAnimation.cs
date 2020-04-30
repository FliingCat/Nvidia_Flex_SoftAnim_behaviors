﻿using UnityEngine;
using System.Collections.Generic;
using uFlex;

#if UNITY_EDITOR
using UnityEditor; // required to build assets
#endif

public class VertMapAssetBuilder
{
    public FlexShapeMatching flexShapeMatching;
    public VertMapAsset vertMapAsset;
    public SkinnedMeshRenderer skin;

    
    public VertMapAssetBuilder(FlexShapeMatching flexShapeMatching, VertMapAsset vertMapAsset, SkinnedMeshRenderer skin)
    {
        this.flexShapeMatching = flexShapeMatching;
        this.vertMapAsset = vertMapAsset;
        this.skin = skin;
    }

    private List<Vector3> GetFlexShapeMatchingPositions()
    {
        List<Vector3> vertPos = new List<Vector3>();
        //FlexShapeMatching[] shapesGOs = FindObjectsOfType<FlexShapeMatching>();

        FlexShapeMatching shapes = this.flexShapeMatching;
        int shapeIndex = 0;
        int shapeIndexOffset = shapes.m_shapesIndex;
        int shapeStart = 0;

        for (int s = 0; s < shapes.m_shapesCount; s++)
        {
            shapeIndex++;
            int shapeEnd = shapes.m_shapeOffsets[s];
            for (int i = shapeStart; i < shapeEnd; ++i)
            {
                Vector3 pos = flexShapeMatching.m_shapeRestPositions[i] + shapes.m_shapeCenters[s];
                vertPos.Add(pos);
                shapeIndexOffset++;
            }

            shapeStart = shapeEnd;
        }

        return vertPos;
    }

    Vector3 RoundVec(Vector3 value)
    {
        float dp = 100000.0f;
        value.x = Mathf.RoundToInt(value.x * dp) / dp;
        value.y = Mathf.RoundToInt(value.y * dp) / dp;
        value.z = Mathf.RoundToInt(value.z * dp) / dp;
        return value;
    }

    private int GetNearestVertIndex(Vector3 particlePos, ref Vector3[] cachedVertices)
    {
        float nearestDist = float.MaxValue;
        int nearestIndex = -1;
        for (int i = 0; i < cachedVertices.Length; i++)
        {
            float dist = Vector3.Distance(particlePos, cachedVertices[i]);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }

    private void SetBoneWeights(ref List<Vector3> uniqueParticlePositions, ref List<int> uniqueParticleIndices)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = skin;

        // Cache used values rather than accessing straight from the mesh on the loop below
        Vector3[] cachedVertices = skinnedMeshRenderer.sharedMesh.vertices;
        Matrix4x4[] cachedBindposes = skinnedMeshRenderer.sharedMesh.bindposes;
        BoneWeight[] cachedBoneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;

        // Make a CWeightList for each bone in the skinned mesh
        WeightList[] nodeWeights = new WeightList[skinnedMeshRenderer.bones.Length];
        for (int i = 0; i < skinnedMeshRenderer.bones.Length; i++)
        {
            nodeWeights[i] = new WeightList();
            nodeWeights[i].boneIndex = i;
            nodeWeights[i].transform = skinnedMeshRenderer.bones[i];
        }

        for (int uniqueIndex = 0; uniqueIndex < uniqueParticleIndices.Count; uniqueIndex++)
        {
            Vector3 particlePos = uniqueParticlePositions[uniqueIndex];
            int i = GetNearestVertIndex(particlePos, ref cachedVertices);
            //Debug.Log(i);
            vertMapAsset.nearestVertIndex.Add(i);
            vertMapAsset.uniqueIndex.Add(uniqueIndex);
            BoneWeight bw = cachedBoneWeights[i];

            if (bw.weight0 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex0].MultiplyPoint3x4(particlePos);// cachedVertices[i]);
                nodeWeights[bw.boneIndex0].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight0));
            }
            if (bw.weight1 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex1].MultiplyPoint3x4(particlePos);//cachedVertices[i]);
                nodeWeights[bw.boneIndex1].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight1));
            }
            if (bw.weight2 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex2].MultiplyPoint3x4(particlePos);//cachedVertices[i]);
                nodeWeights[bw.boneIndex2].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight2));
            }
            if (bw.weight3 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex3].MultiplyPoint3x4(particlePos);//cachedVertices[i]);
                nodeWeights[bw.boneIndex3].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight3));
            }
        }

        vertMapAsset.particleNodeWeights = nodeWeights;
        //Debug.Log(nodeWeights.Length);
    }

    public void CreateAsset()
    {
        vertMapAsset.vertexParticleMap = new List<int>();

        List<Vector3> particlePositions = GetFlexShapeMatchingPositions();
        List<Vector3> uniqueParticlePositions = new List<Vector3>();
        List<int> uniqueParticleIndices = new List<int>();
        for (int i = 0; i < particlePositions.Count; i++)
        {
            Vector3 vert = particlePositions[i];
            vert = RoundVec(vert);

            if (uniqueParticlePositions.Contains(vert) == false)
            {
                uniqueParticleIndices.Add(i);
                uniqueParticlePositions.Add(vert);
            }


            vertMapAsset.vertexParticleMap.Add(uniqueParticlePositions.IndexOf(vert));
        }

        vertMapAsset.particleRestPositions = uniqueParticlePositions;
        vertMapAsset.nearestVertIndex = new List<int>();
        vertMapAsset.uniqueIndex = new List<int>();
        //Debug.Log(uniqueParticlePositions.Count);
        //Debug.Log(uniqueParticleIndices.Count);
        SetBoneWeights(ref uniqueParticlePositions, ref uniqueParticleIndices);

        // trigger save
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(vertMapAsset);
#endif
    }
}

public class FlexAnimation : FlexProcessor
{
    public bool rebuildVertMapAsset = false;
    public VertMapAsset vertMapAsset;
    public SkinnedMeshRenderer skinnedMeshRenderer;
    private FlexShapeMatching flexShapeMatching;
    private bool firstRun = true;
    //private Vector3[] particlePositions;
    public Vector3[] particlePositions;
    public bool drawVertMapAsset = false;
    public GameObject shapeMatchingGO;
    
    //public List<FlexParticles> m_flexGameObjects = new List<FlexParticles>();
    //public List<FlexShapeMatching> shapesGOList = new List<FlexShapeMatching>();

    Vector3[] _cachedVertices;
    Matrix4x4[] _cachedBindposes;
    BoneWeight[] _cachedBoneWeights;
    float refreshRate = 1.0f / 30.0f;
    float timeDelta;

    void OnEnable()
    {

        if (flexShapeMatching == null)
            flexShapeMatching = shapeMatchingGO.GetComponent<FlexShapeMatching>();
        //print(particlePositions.Length);
    }

    public override void PreContainerUpdate(FlexSolver solver, FlexContainer cntr, FlexParameters parameters)
    {
        if (enabled == false)
            return;

        if (firstRun)
        {
            firstRun = false;

            _cachedVertices = skinnedMeshRenderer.sharedMesh.vertices;
            _cachedBindposes = skinnedMeshRenderer.sharedMesh.bindposes;
            _cachedBoneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;

            bool createAsset = vertMapAsset == null || rebuildVertMapAsset;

            if (vertMapAsset == null)
            {
#if UNITY_EDITOR
                vertMapAsset = ScriptableObject.CreateInstance<VertMapAsset>();
                AssetDatabase.CreateAsset(vertMapAsset, "Assets/" + this.name + "VertMapAsset.asset");
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = vertMapAsset;
#endif
            }

            if (createAsset)
            {
                vertMapAsset.vertexParticleMap = new List<int>();
                VertMapAssetBuilder vertMapAssetBuilder = new VertMapAssetBuilder(flexShapeMatching, vertMapAsset, skinnedMeshRenderer);
                vertMapAssetBuilder.CreateAsset();
            }

            particlePositions = new Vector3[vertMapAsset.particleRestPositions.Count];
            //print(particlePositions.Length);
            //Debug.Log(this.skinnedMeshRenderer.name);
            UpdateParticlePositions();

        }
        else
        {
            if (timeDelta < refreshRate)
                return;

            // Only process once 30 times a second
            
            UpdateParticlePositions();
            MatchShapes(); // apply to soft body

            while (timeDelta >= refreshRate)
                timeDelta -= refreshRate;
        }
    }

    public void UpdateParticlePositions()
    {
        //Debug.Log("particle positions length: " + particlePositions.Length);
        //print(particlePositions.Length);
        for (int i = 0; i < particlePositions.Length; i++)
        {
            particlePositions[i] = Vector3.zero;
        }

        // Now get the local positions of all weighted indices...
        foreach (WeightList wList in vertMapAsset.particleNodeWeights)
        {
            foreach (VertexWeight vw in wList.weights)
            {
                Transform t = skinnedMeshRenderer.bones[wList.boneIndex];
                //Debug.Log(particlePositions[vw.index]);
                particlePositions[vw.index] += t.localToWorldMatrix.MultiplyPoint3x4(vw.localPosition) * vw.weight;
                //print(particlePositions[vw.index]);
            }
        }

        //Debug.Log(vertMapAsset.particleNodeWeights.Length);
        // Now convert each point into local coordinates of this object.
        for (int i = 0; i < particlePositions.Length; i++)
        {
            particlePositions[i] = transform.InverseTransformPoint(particlePositions[i]);
        }
        //print("Particle POS:" + particlePositions[1]);


       

    }

    private void MatchShapes()
    {
        FlexShapeMatching shapes = this.flexShapeMatching;
        //FlexShapeMatching[] shapesGOs = FindObjectsOfType<FlexShapeMatching>();
        //FlexShapeMatching[] shapes_Array = new FlexShapeMatching[m_flexGameObjects.Count];

        //Debug.Log("shape name: " + shapes.name + " shapes count: " + shapes.m_shapesCount + " shape's Index: " + shapes.m_shapesIndex);
        //for (int i = 0; i < shapesGOs.Length; i++) { Debug.Log("shape game object name: " + shapesGOs[i].name); }

        //Debug.Log( "Number of shapes game objects: " + shapesGOs.Length);
        int shapeIndex = 0;
        int shapeIndexOffset = 0; //no need for offsets, shapes.m_shapesIndex;
        ////Debug.Log("before for loop: " + shapes.m_shapesIndex);
        int shapeStart = 0;
        ////Debug.Log("shape name: " + shapes.name + " shape's index: " + shapes.m_shapesIndex);
        int vertIndex = 0;

        //foreach (FlexShapeMatching shapeGo_temp in shapesGOs)
        ////for (int i = 0; i < shapesGOs.Length; i++)
        //{
        //    //Debug.Log(shapeGo_temp.name);
        //    //Debug.Log(shapeGo_temp.m_shapesIndex);
        //    int shapeIndex = 0;

        //    int shapeStart = 0;


        //    int shapeIndexOffset = shapeGo_temp.m_shapesIndex;

        //    int vertIndex = 0;
        //    //Debug.Log(shapeIndexOffset);
        //    for (int s = 0; s < shapeGo_temp.m_shapesCount; s++)
        //    {
        //        Vector3 shapeCenter = new Vector3();
        //        shapeIndex++;
        //        //Debug.Log("shape name: " + shapes.name + " shape offsets: " + shapes.m_shapeOffsets[s]);
        //        int shapeEnd = shapeGo_temp.m_shapeOffsets[s];

        //        int shapeCount = shapeEnd - shapeStart;
        //        //Debug.Log("shape count: " + shapeCount);
        //        int origShapeIndexOffset = shapeIndexOffset;
        //        for (int i = shapeStart; i < shapeEnd; ++i)
        //        {
        //            int mappedIndex = vertMapAsset.vertexParticleMap[vertIndex];
        //            Vector3 pos = particlePositions[mappedIndex];
        //            shapeGo_temp.m_shapeRestPositions[shapeIndexOffset] = pos;
        //            //Debug.Log(pos);
        //            shapeCenter += pos;
        //            //Debug.Log("shape Index: " + shapeIndex);
        //            //Debug.Log("shape Index offset: " + shapeIndexOffset);
        //            //Debug.Log("shape center postion: " + shapeCenter);
        //            shapeIndexOffset++;
        //            vertIndex++;
        //        }

        //        shapeCenter /= shapeCount;

        //        for (int i = shapeStart; i < shapeEnd; ++i)
        //        {
        //            Vector3 pos = shapeGo_temp.m_shapeRestPositions[origShapeIndexOffset];
        //            pos -= shapeCenter;
        //            shapeGo_temp.m_shapeRestPositions[origShapeIndexOffset] = pos;
        //            origShapeIndexOffset++;
        //        }

        //        shapeStart = shapeEnd;
        //    }
        //}

        for (int s = 0; s < shapes.m_shapesCount; s++)
        {
            Vector3 shapeCenter = new Vector3();
            shapeIndex++;
            //Debug.Log("count: " + s + "shape index count : " + shapeIndexOffset);
            //Debug.Log("shape name: " + shapes.name + " shape offsets: " + shapes.m_shapeOffsets[s]);
            int shapeEnd = shapes.m_shapeOffsets[s];
            //Debug.Log(shapeEnd);

            int shapeCount = shapeEnd - shapeStart;
            //Debug.Log("shape count: " + shapeCount);
            int origShapeIndexOffset = shapeIndexOffset;
            for (int i = shapeStart; i < shapeEnd; ++i)
            {
                int mappedIndex = vertMapAsset.vertexParticleMap[vertIndex];
                Vector3 pos = particlePositions[mappedIndex];
                shapes.m_shapeRestPositions[shapeIndexOffset] = pos;
                //Debug.Log(pos);
                shapeCenter += pos;
                //Debug.Log("shape Index: " + shapeIndex);
                //Debug.Log("shape Index offset: " + shapeIndexOffset);
                //Debug.Log("shape center postion: " + shapeCenter);
                shapeIndexOffset++;
                vertIndex++;
                //Debug.Log("vertex index: " + vertIndex);
            }

            shapeCenter /= shapeCount;

            for (int i = shapeStart; i < shapeEnd; ++i)
            {
                Vector3 pos = shapes.m_shapeRestPositions[origShapeIndexOffset];
                pos -= shapeCenter;
                shapes.m_shapeRestPositions[origShapeIndexOffset] = pos;
                origShapeIndexOffset++;
            }

            shapeStart = shapeEnd;
        }
    }

    public void Update()
    {
        timeDelta += Time.deltaTime;

    }

    public virtual void OnDrawGizmos()
    {

        if (drawVertMapAsset && vertMapAsset != null)
        {
            float boxSize = 0.2f;

            //Gizmos.color = Color.red;
            //foreach (Vector3 vert in vertMapAssetBuilder._cachedVertices)
            //{
            //    Gizmos.DrawCube(vert, new Vector3(boxSize, boxSize, boxSize));
            //}

            //Gizmos.color = Color.blue;
            //foreach (Vector3 vert in vertMapAssetBuilder._uniqueParticlePositions)
            //{
            //    Gizmos.DrawCube(vert, new Vector3(boxSize, boxSize, boxSize));
            //}
            //Gizmos.color = Color.red;
            //foreach (WeightList wList in vertMapAsset.particleNodeWeights)
            //{
            //    foreach (VertexWeight vw in wList.weights)
            //    {
            //        Transform t = skinnedMeshRenderer.bones[wList.boneIndex];
            //        //Debug.Log(particlePositions[vw.index]);
            //        particlePositions[vw.index] += t.localToWorldMatrix.MultiplyPoint3x4(vw.localPosition) * vw.weight;
            //        Gizmos.DrawCube(particlePositions[vw.index], new Vector3(boxSize, boxSize, boxSize));
            //    }
            //}

            //if (vertMapAsset.particleRestPositions != null)
            //{
            //    Gizmos.color = Color.red;
            //    foreach (Vector3 vert in vertMapAsset.particleRestPositions)
            //    {
            //        Gizmos.DrawCube(vert, new Vector3(boxSize, boxSize, boxSize));
            //    }
            //}


            //Gizmos.color = Color.red;
            //foreach (Vector3 vert in skinnedMeshRenderer.sharedMesh.vertices)
            //{
            //    Gizmos.DrawCube(vert, new Vector3(boxSize, boxSize, boxSize));
            //}

            if (particlePositions != null)
            {
                Gizmos.color = Color.red;
                foreach (Vector3 vert in particlePositions)
                {
                    Gizmos.DrawCube(vert, new Vector3(boxSize, boxSize, boxSize));
                }
            }

        }

    }

}
