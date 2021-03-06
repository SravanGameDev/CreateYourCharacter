using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SimpleVisualizer;


namespace SVS
{
    public class Visualizer : MonoBehaviour
    {
        public LSystemsGenerator LSystemsGenerator;
        List<Vector3> positions = new List<Vector3>();

        /* These are used to path insted of roads
        public GameObject prefab;
        public Material lineMaterial;
        */
        
        public int roadLength = 8;

        public RoadHelper roadHelper;
        public StructureHelper structureHelper;


        private int length = 8;
        private float angle = 90;

        private bool waitingForTheRoad = false;

        public int Length
        {
            get
            {
                if (length > 0)
                {
                    return length;
                }
                else
                {
                    return 1;
                }
            }
            set => length = value;
        }

        private void Start()
        {
            roadHelper.finishedCoroutine += () => waitingForTheRoad = false;
            CreateTown();
        }

        public void CreateTown()
        {
            length = roadLength;
            roadHelper.Reset();
            structureHelper.Reset();
            var sequence = LSystemsGenerator.GenerateSentence();
            StartCoroutine(VisualizeSequence(sequence));
        }


        private IEnumerator VisualizeSequence(string sequence)
        {
            Stack<AgentParameters> savePoints = new Stack<AgentParameters>();
            var currentPosition = Vector3.zero;

            Vector3 direction = Vector3.forward;
            Vector3 tempPosition = Vector3.zero;

            positions.Add(currentPosition);

            foreach (var letter in sequence)
            {
                if (waitingForTheRoad)
                {
                    yield return new WaitForEndOfFrame();
                }

                EncodingLetters encoding = (EncodingLetters)letter;

                switch (encoding)
                {
                    case EncodingLetters.unknown:
                        break;

                    case EncodingLetters.save:
                        savePoints.Push(new AgentParameters
                        {
                            position = currentPosition,
                            direction = direction,
                            length = Length
                        });
                        break;

                    case EncodingLetters.load:
                        if (savePoints.Count > 0)
                        {
                            var agentParameter = savePoints.Pop();
                            currentPosition = agentParameter.position;
                            direction = agentParameter.direction;
                            Length = agentParameter.length;
                        }
                        else
                        {
                            throw new System.Exception("Donot have save point in the stack");
                        }
                        break;

                    case EncodingLetters.draw:
                        tempPosition = currentPosition;
                        currentPosition += direction * length;
                        //DrawLine(tempPosition, currentPosition, Color.red);
                        StartCoroutine(roadHelper.PlaceStreetPosition(tempPosition, Vector3Int.RoundToInt(direction), length));

                        waitingForTheRoad = true;
                        yield return new WaitForEndOfFrame();

                        Length -= 2;
                        positions.Add(currentPosition);
                        break;

                    case EncodingLetters.turnRight:
                        direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                        break;

                    case EncodingLetters.turnLeft:
                        direction = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
                        break;

                    default:
                        break;
                }
            }

            /* Logic changed and Instantiate change to a different section
            foreach (var position in positions)
            {
                Instantiate(prefab, position, Quaternion.identity);
            }
            */

            yield return new WaitForSeconds(0.1f);
            roadHelper.FixRoad();
            yield return new WaitForSeconds(0.8f);
            StartCoroutine(structureHelper.PlaceStructuresAroundRoad(roadHelper.GetRoadPositions()));
        }
    }
}

