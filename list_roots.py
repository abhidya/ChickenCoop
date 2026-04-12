import yaml
import re

with open('Assets/Scenes/MainGame.unity', 'r') as f:
    content = f.read()

# Very hacky unity yaml parser for roots
docs = content.split('--- !u!')
transforms = {}
gameobjects = {}

for doc in docs:
    if not doc.strip(): continue
    lines = doc.split('\n')
    header = lines[0]
    
    if header.startswith('4 &'):
        # Transform
        tid = header.split('&')[1].strip()
        father = None
        go_id = None
        for line in lines:
            if 'm_Father:' in line:
                m = re.search(r'fileID:\s*(\d+)', line)
                if m: father = m.group(1)
            if 'm_GameObject:' in line:
                m = re.search(r'fileID:\s*(\d+)', line)
                if m: go_id = m.group(1)
        transforms[tid] = {'father': father, 'go': go_id}
        
    elif header.startswith('1 &'):
        # GameObject
        goid = header.split('&')[1].strip()
        name = ""
        for line in lines:
            if 'm_Name:' in line:
                name = line.split('m_Name:')[1].strip()
        gameobjects[goid] = name

roots = []
for tid, t in transforms.items():
    if t['father'] == '0':
        if t['go'] in gameobjects:
            roots.append(gameobjects[t['go']])

print("Roots:")
for r in roots:
    print(r)
