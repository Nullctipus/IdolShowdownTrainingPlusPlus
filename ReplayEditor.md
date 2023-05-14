# Replay Editor

## Usage
Open the menu and enable the recording editor

type the code in the Text Editor section

click run

## Format
`input,frames,comment`

the input is any of these characters `123456789lmh` any other character is ignored. meaning `1extral` is valid and the same as `1l`

This does not hold true for the frames section as anything other than a number will cause an error

The comment can contain any character and it will not effect any other parts


## Quirks

##### Do to implementation certain things will result in unexpected behavior
The game processes input with up, down, left, right. This means that inputting 24 is the same as 1, and vice versa, so 14 is equivilent to 2

The way that inputs are read is a bitmask, so ll is the same as no l at all. This applys to all inputs

Because 5 is the same as no input `,1` is functionaly the same as `5,1`

## Examples

Botan basic combo

```
m,1
5,35
m,1
5,20
h,1
5,40
2h,1,Launcher
5,12
2,2
1,3
4,2
m,1
5,90,Wait to catch
l,1,Catch
```

Every Input
```
1,1
2,1
3,1
4,1
5,1
6,1
7,1
8,1
9,1
l,1
m,1,
h,1
lm,1
lh,1
mlh,1
```

Quirk Example

```
41,1, same as 2
42,1, same as 1
78,1, same as 4
lmhml,1, same as h
,1,This is just a normal QCB input
```