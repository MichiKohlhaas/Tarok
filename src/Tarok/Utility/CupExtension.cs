namespace Tarok.Utility;

public static class CupExtension
{
    extension(int rank)
    {
        public char ToLetter(bool isReversed = false)
        {
            var ch = rank switch
            {
                2 => isReversed ? 'A' : 'N',
                3 => isReversed ? 'B' : 'O',
                4 => isReversed ? 'C' : 'P',
                5 => isReversed ? 'D' : 'Q',
                6 => isReversed ? 'E' : 'R',
                7 => isReversed ? 'F' : 'S',
                8 => isReversed ? 'G' : 'T',
                9 => isReversed ? 'H' : 'U',
                10 => isReversed ? 'I' : 'V',
                11 => isReversed ? 'J' : 'W',
                12 => isReversed ? 'K' : 'X',
                13 => isReversed ? 'L' : 'Y',
                14 => isReversed ? 'M' : 'Z',
                _ => '\0'    
            };
            return ch;
        }
    }
}