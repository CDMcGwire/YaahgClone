# Storyboard Parsing Logic

## Sample

```text
# Tavern

# All storyboards must have an Intro
-> Intro
@backdrop=path/to/backdrop
| This is where narration text goes.
| New lines are preserved and grouped.
| Unless there are more than 3.
| This would start a new panel.

| A blank line also begins a new panel.
# Writing a set of stat changes includes them with the last panel.
? This is a decision prompt
- This is the first choice -< PanelSet02
- This is the second choice -< PanelSet03
[Character has trait "Special" and per < 3]
- This is a conditional choice -< SecretPanelSet
```

## Preprocessor

```text
while lineQueue has lines
 if line is comment
  discard
 else
  if line empty
   while next is empty
    discard
   enqueue line onto finalQueue
```

## Parser

```text
while finalQueue has lines
 dequeue line from finalQueue
 if line starts with "->"
  get name value from line
  get panel set object from parsePanelSet(name, finalQueue)
  add panel to dictionary
 else if empty
  discard
 else
  throw error "Invalid syntax"
return dictionary of panels
```

## Parse Panel Set

```text
instantiate panelSet
while queue has lines
 if next in queue is empty
  return panelSet
 else if next in queue starts with @
  get reference from parse reference
  add reference to panelSet Panel sets will store references in order with the last panel added
 else if next in queue starts with "|"
  get panel from parseNarrationPanel
  add panel to panelSet
 else if next in queue starts with "?"
  get panel from parseDecisionPanel
  add panel to panelSet
  return panelSet
 else if next in queue starts with "- " or "-<"
  get panel from parseBranch
  add branch to panelSet
  return panelSet
```
