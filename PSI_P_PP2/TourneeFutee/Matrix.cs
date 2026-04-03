using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Matrix
    {
        public List<List<float>> donnees;
        public float valeurParDefaut;

        public Matrix(int nbRows = 0, int nbColumns = 0, float defaultValue = 0)
        {
            if (nbRows < 0 || nbColumns < 0) throw new ArgumentOutOfRangeException();

            valeurParDefaut = defaultValue;
            donnees = new List<List<float>>();

            for (int i = 0; i < nbRows; i++)
            {
                List<float> ligne = new List<float>();
                for (int j = 0; j < nbColumns; j++)
                {
                    ligne.Add(valeurParDefaut);
                }
                donnees.Add(ligne);
            }
        }

        public float DefaultValue
        {
            get { return valeurParDefaut; }
        }

        public int NbRows
        {
            get { return donnees.Count; }
        }

        public int NbColumns
        {
            get
            {
                if (donnees.Count > 0) return donnees[0].Count;
                return 0;
            }
        }

        public void AddRow(int i)
        {
            if (i < 0 || i > NbRows) throw new ArgumentOutOfRangeException();

            List<float> nouvelleLigne = new List<float>();
            int colonnes = NbColumns;
            for (int j = 0; j < colonnes; j++)
            {
                nouvelleLigne.Add(valeurParDefaut);
            }
            donnees.Insert(i, nouvelleLigne);
        }

        public void AddColumn(int j)
        {
            if (j < 0 || j > NbColumns) throw new ArgumentOutOfRangeException();

            for (int k = 0; k < donnees.Count; k++)
            {
                donnees[k].Insert(j, valeurParDefaut);
            }
        }

        public void RemoveRow(int i)
        {
            if (i < 0 || i >= NbRows) throw new ArgumentOutOfRangeException();
            donnees.RemoveAt(i);
        }

        public void RemoveColumn(int j)
        {
            if (j < 0 || j >= NbColumns) throw new ArgumentOutOfRangeException();
            for (int k = 0; k < donnees.Count; k++)
            {
                donnees[k].RemoveAt(j);
            }
        }

        public float GetValue(int i, int j)
        {
            if (i < 0 || i >= NbRows || j < 0 || j >= NbColumns) throw new ArgumentOutOfRangeException();
            return donnees[i][j];
        }

        public void SetValue(int i, int j, float v)
        {
            if (i < 0 || i >= NbRows || j < 0 || j >= NbColumns) throw new ArgumentOutOfRangeException();
            donnees[i][j] = v;
        }

        public void Print()
        {
            for (int i = 0; i < NbRows; i++)
            {
                for (int j = 0; j < NbColumns; j++)
                {
                    Console.Write(donnees[i][j] + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}