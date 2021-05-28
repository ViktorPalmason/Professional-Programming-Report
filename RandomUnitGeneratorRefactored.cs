using System.Collections.Generic;
using System.Linq;
using TbsFramework.Cells;
using TbsFramework.Units;
using UnityEngine;

namespace TbsFramework.Grid.UnitGenerators
{
    class RandomUnitGenerator : MonoBehaviour, IUnitGenerator
    {
        private System.Random _rnd = new System.Random();

        #pragma warning disable 0649
        public Transform UnitsParent;

        public GameObject UnitPrefab;
        public int NumberOfPlayers;
        public int UnitsPerPlayer;
        #pragma warning restore 0649

        /// <summary>
        /// Method spawns UnitPerPlayer nunmber of UnitPrefabs in random positions.
        /// Each player gets equal number of units.
        /// </summary>
        public List<Unit> SpawnUnits(List<Cell> cells)
        {
            List<Unit> ret = new List<Unit>();

            List<Cell> freeCells = cells.FindAll(h => h.GetComponent<Cell>().IsTaken == false);
            freeCells = freeCells.OrderBy(h => _rnd.Next()).ToList();

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                for (int j = 0; j < UnitsPerPlayer; j++)
                {
                    var cell = freeCells.ElementAt(0);
                    freeCells.RemoveAt(0);
                    cell.GetComponent<Cell>().IsTaken = true;
                    // modified so all units are made children of Units when instantiated
                    // this makes it so that they are placed at correct postion and have the correct rotation
                    var unit = Instantiate(UnitPrefab, UnitsParent);
                    //unit.transform.parent = UnitsParent.transform;
                    //unit.transform.rotation = UnitsParent.transform.rotation;
                    unit.transform.position = cell.transform.position + new Vector3(0, 0, 0);
                    unit.GetComponent<Unit>().PlayerNumber = i;
                    unit.GetComponent<Unit>().Cell = cell.GetComponent<Cell>();
                    unit.GetComponent<Unit>().Initialize();

                    ret.Add(unit.GetComponent<Unit>());
                }
            }
            //UnitsParent.transform.Rotate(new Vector3(90f, 0f, 0f), Space.Self);
            return ret;
        }
    }
}
