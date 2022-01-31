# wordle

A simple wordle helper written in C#.

## Single Guess Mode

Enter the word dictionary file name, the template of your guess, chars to exclude and chars to include.
The solver prints all possible matched words.

Parameters:

- file name of dictionary with 1 word per line
- template: use '_' for unknown characters, use letters for known (green) characters, use '[chars]' for yellow chars.
- list of chars that are NOT in the target (grey characters)
- list of chars that are known to be in the target, but unknown location (yellow characters)

All parameters are required, use an empty string ("") on the command line if needed.

Example Usage:

```
bin/Debug/net5.0/wordle.exe words_alpha.txt s[r]_a_ tinpycf ru
```

Yields:

```
serau
sugar
surah
sural
suras
```

## Interactive Mode

Enter a series of your guess/result pairs. After each pair the solver will print the remaining possible words.
The 'guess' is a the word you guessed. Must be contained in the dictionary.
The 'result' is a string of chars representing the colors indicated by your guess in wordle. 

Where the chars to color map is:
   b - black
   y - yellow
   g - green

The program exits when there is only 1 possible word left.

Here's an example interactive session:

```
Enter your guessed word, followed by the colors returned for your guess.
Valid colors are b - black; y - yellow; g - green.
>
stain byybb
abbot
abdat
ablet
...
wheat
wreat
zayat
zakat
538
Enter your guessed word, followed by the colors returned for your guess.
Valid colors are b - black; y - yellow; g - green.
>
vocal bybgy
bloat
float
gloat
lutao
oblat
ploat
talao
7
Enter your guessed word, followed by the colors returned for your guess.
Valid colors are b - black; y - yellow; g - green.
>
oblat yyygg
bloat
1
```