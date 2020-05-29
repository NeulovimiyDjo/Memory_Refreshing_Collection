import sys
import xml.etree.cElementTree as ET
from xml.etree.cElementTree import Element

# usage test: filter_and_split_fias_xml.py fias_test.xml 8
# usage real: filter_and_split_fias_xml.py fias.xml 3000000
mainFileName = sys.argv[1]
step = int(sys.argv[2])


def getFilteredElement():
    context = iter(ET.iterparse(mainFileName, events=('start', 'end')))
    _, root = next(context)
    
    i = 0
    for event, elem in context:
        i += 1
        
        if event == 'end' and elem.tag == "Object" and elem.get('AOLEVEL') == '4': # only citiees (level 4)
        
            newElem = Element('Object')
            newElem.set('FORMALNAME', elem.get('FORMALNAME'))
            newElem.set('AOLEVEL', elem.get('AOLEVEL'))
            newElem.set('PLAINCODE', elem.get('PLAINCODE'))        
            
            root.clear()
            yield newElem
            
        if i % 100000 == 1: # print progress for every 100'000 objects
            print("Iterated initial objects: " + str(i))

    print("Total iterated initial objects: " + str(i))


index = 0
filename = None
f = None
for elem in getFilteredElement():
    if elem.tag == 'Object':
        index += 1
        if index % step == 1: # create new file every 'step' elements
            if index != 1: # close previous root tag
                f.write(b"</AddressObjects>\n")
            
            filename = format(mainFileName.replace(".xml", "") + str(index // step + 1) + ".xml")
            f = open(filename, 'wb')         
            f.write(b"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n")
            f.write(b"<AddressObjects>\n")

        f.write(ET.tostring(elem, encoding='utf-8'))
        f.write(b"\n")
        
        if index % 1000 == 1: # print progress for every 1'000 objects
            print("Written filtered objects: " + str(index))
        
f.write(b"</AddressObjects>")

print("Total written filtered objects: " + str(index))
