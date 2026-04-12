import re

with open('Assets/Scenes/MainGame.unity', 'r') as f:
    content = f.read()

docs = content.split('--- !u!')
transforms = {}
gameobjects = {}

for doc in docs:
    if not doc.strip(): continue
    lines = doc.split('\n')
    header = lines[0]
    
    if header.startswith('4 &'):
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
        goid = header.split('&')[1].strip()
        name = ""
        components = []
        for line in lines:
            if 'm_Name:' in line:
                name = line.split('m_Name:')[1].strip()
        gameobjects[goid] = {'name': name, 'components': [], 'id': goid}

for tid, t in transforms.items():
    if t['father'] == '0' and t['go'] in gameobjects:
        go = gameobjects[t['go']]
        print(f"Root: {go['name']} (ID: {go['id']})")
        
