# wordle

A simple wordle helper written in C#.

Parameters:

- file name of dictionary with 1 word per line
- template: use '_' for unknown characters, use letters for known (green) characters
- list of chars that are NOT in the target (grey characters)
- list of chars that are known to be in the target, but unknown location (yellow characters)

All parameters are required, use an empty string ("") on the command line if needed.

Example Usage:

```
bin/Debug/net5.0/wordle.exe words_alpha.txt s__a_ tinpycf ru
```

Yields:

```
serau
sugar
surah
sural
suras
```
