# PrefabHouseTools

PrefabHouseTools is a [Revit](https://www.autodesk.com/products/revit/overview) addin
for several automation design task in the design process of prefabricated houses.
That include new houses and renovation of existing building.
It mainly target the interior design of dwellings.
This toolkit contain following project.

Remark:
The current project only work on Revit2020.Support for multiple version will be available soon.

##1-InteriorAutoPanel
    Generate prefabricated interior wall panel for any room.User can define the panel unit width and height,
    this tool will generate panels to avoid windows and doors and try to use full panel as often as possible.
    
    This fuction rely on a custom family file PanelAuto.rfa to work.
    Furture version may integrate that into the addin.

##2-AutoRouteMEP(WIP)
    Generate MEP pipes automatically from givin fixtures and equipment.

###2.1 AutoRouteMEP-Electrical
    Generate electrical lines from electrical fixtures(including 
    lighting fixtures,outlet fixtures,HVAC equipment etc.) and
    panel equipment.
    The electrical wire generated is represented as modelline in the rvt file.
