using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneticAlgo : MonoBehaviour
{

    [Header("Genetic Algorithm parameters")]
    public int popSize = 100;
    public int popSizePredator = 20;
    public GameObject animalPrefab;
    public GameObject predatorPrefab;

    public int numberAnimals;
    public int numberPredatos;


    [Header("Dynamic elements")]
    public float vegetationGrowthRate = 1.0f;
    public float currentGrowth;

    private List<GameObject> animals;
    private List<GameObject> predators;
    protected Terrain terrain;
    protected CustomTerrain customTerrain;
    protected float width;
    protected float height;
    private int[,] details = null;

    void Start()
    {
        // Retrieve terrain.
        terrain = Terrain.activeTerrain;
        customTerrain = GetComponent<CustomTerrain>();
        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;

        // Initialize terrain growth.
        currentGrowth = 0.0f;

        // Initialize animals array.
        animals = new List<GameObject>();
        predators = new List<GameObject>();

        for (int i = 0; i < popSize; i++)
        {
            GameObject animal = makeAnimal();
            animals.Add(animal);
        }

        for (int i = 0; i < popSizePredator; i++)
        {
            GameObject predator = makePredator();
            predators.Add(predator);
        }
    }

    void Update()
    {
        // Keeps animal to a minimum.
        while (animals.Count < popSize / 2)
        {
            animals.Add(makeAnimal());
        }
        //customTerrain.debug.text = "N� animals: " + animals.Count.ToString();

        while (predators.Count < popSizePredator / 2)
        {
            predators.Add(makePredator());
        }
        //customTerrain.debug.text = "N� predators: " + predators.Count.ToString();

        //Predator.positionAnimals = getAnimalPositions(animals);
        // Update grass elements/food resources.
        numberAnimals = animals.Count;
        numberPredatos = predators.Count;
        updateResources();
    }

    /// <summary>
    /// Method to place grass or other resource in the terrain.
    /// </summary>
    public void updateResources()
    {
        Vector2 detail_sz = customTerrain.detailSize();
        details = customTerrain.getDetails();
        // Debug.Log("details:");
        // Debug.Log(details);
        // Debug.Log("vs width and height");
        // Debug.Log(width);
        // Debug.Log( height);
        currentGrowth += vegetationGrowthRate;
        while (currentGrowth > 1.0f)
        {
            float x = UnityEngine.Random.value;
            float y = UnityEngine.Random.value;
            float terrainHeight = Terrain.activeTerrain.terrainData.GetHeight((int)(x*Terrain.activeTerrain.terrainData.size.x), (int)(y * Terrain.activeTerrain.terrainData.size.y));
            if (terrainHeight < 60)
            {
                details[(int)(y * detail_sz.y), (int)(x * detail_sz.x)] = 1;
                currentGrowth -= 1.0f;
            }
        }
        customTerrain.saveDetails();
    }

    /// <summary>
    /// Method to instantiate an animal prefab. It must contain the animal.cs class attached.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject makeAnimal(Vector3 position)
    {
        GameObject animal = Instantiate(animalPrefab, transform);
        animal.GetComponent<Animal>().Setup(customTerrain, this);
        animal.transform.position = position;
        animal.transform.Rotate(0.0f, UnityEngine.Random.value * 360.0f, 0.0f);
        return animal;
    }

    public GameObject[,] getAnimalPositions()//List<GameObject> animals
    {

        GameObject[,] listPositions = new GameObject[(int)details.GetLength(0), (int)details.GetLength(1)];

        for (int i = 0; i < animals.Count; i++)
        {
            GameObject animal = animals[i];
            Vector3 position = animal.transform.position;
            listPositions[(int)position.z, (int)position.x] = animal; // on a interverti x et z ! 
        }
        return listPositions;
    }

    public GameObject makePredator(Vector3 position)
    {
        GameObject predator = Instantiate(predatorPrefab, transform);
        predator.GetComponent<Predator>().Setup(customTerrain, this);
        predator.transform.position = position;
        predator.transform.Rotate(0.0f, UnityEngine.Random.value * 360.0f, 0.0f);
        return predator;
    }

    /// <summary>
    /// If makeAnimal() is called without position, we randomize it on the terrain.
    /// </summary>
    /// <returns></returns>
    public GameObject makeAnimal()
    {
        Vector3 scale = terrain.terrainData.heightmapScale;
        float x = UnityEngine.Random.value * width;
        float z = UnityEngine.Random.value * height;
        float y = customTerrain.getInterp(x / scale.x, z / scale.z);
        return makeAnimal(new Vector3(x, y, z));
    }

    public GameObject makePredator()
    {
        Vector3 scale = terrain.terrainData.heightmapScale;
        float x = UnityEngine.Random.value * width;
        float z = UnityEngine.Random.value * height;
        float y = customTerrain.getInterp(x / scale.x, z / scale.z);
        return makePredator(new Vector3(x, y, z));
    }

    /// <summary>
    /// Method to add an animal inherited from anothed. It spawns where the parent was.
    /// </summary>
    /// <param name="parent"></param>
    public void addOffspring(Animal parent)
    {
        GameObject animal = makeAnimal(parent.transform.position);
        animal.GetComponent<Animal>().InheritBrain(parent.GetBrain(), true);
        animals.Add(animal);
    }

    public void addOffspring_predator(Predator parent)
    {
        GameObject predator = makePredator(parent.transform.position);
        predator.GetComponent<Predator>().InheritBrain(parent.GetBrain(), true);
        predators.Add(predator);
    }

    /// <summary>
    /// Remove instance of an animal.
    /// </summary>
    /// <param name="animal"></param>
    public void removeAnimal(Animal animal)
    {
        animals.Remove(animal.transform.gameObject);
        Destroy(animal.transform.gameObject);
    }
    public void removeAnimal(GameObject animal)
    {
        animals.Remove(animal.transform.gameObject);
        Destroy(animal.transform.gameObject);
    }
    public void removePredator(Predator predator)
    {
        predators.Remove(predator.transform.gameObject);
        Destroy(predator.transform.gameObject);
    }

}
