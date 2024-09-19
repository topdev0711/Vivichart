//Model JS
"use strict";

//----------------------------------------------------------------------------------------------------------------------
// Logging - also in master.js for use elsewhere, but added here for easy way to change logging level
// will be removed from here after dev phase
//----------------------------------------------------------------------------------------------------------------------
var iawDebugLevel = 2; // 0 = No logging, 1 = Errors only, 2 = Logging, 3 = Verbose
var iawError = iawDebugLevel > 0 ? console.error.bind(window.console) : function () { };
var iawLog = iawDebugLevel > 1 ? console.log.bind(window.console) : function () { };
var iawVerbose = iawDebugLevel > 2 ? console.log.bind(window.console) : function () { };
//----------------------------------------------------------------------------------------------------------------------

var MGLB; // global level variable object
var myDiagram; // objet representing the gojs diagram
var AEAList = []; // left hand list of items available to be added to the chart
var NoteEditor; // actual tinymce control for editing notes
var nodeAttrs = new Array(7); // node level line attributes
var modelAttrs = new Array(7); // model level line attributes
var ControlsInitialised = false;
var search = "";
var arr = [];
var linkArr = [];
var zoomSlider;
var resizedNodeWidth;
var resizedNodeHeight;

// The AEA sort variables are used to hold the current sort column and sort direction Asc or Desc
var AEASortColumn = 0;
var AEASortAscending = 'Asc';

/* this will stop the system from moving a node that is not wholly on the chart to the middle
   it will be set on drag/drop and on context menu display  */
var SuppressSelectionMove = false;
var SuppressLayoutComplete = false;

/* Declare Constants */
const LeftToRight = 0;
const TopDown = 90;
const RightToLeft = 180;
const BottomUp = 270;

//$(document).ready(function () {
$(function () {
    // Start keep alive timer
    setTimeout(KeepSessionAlive, 5 * 60 * 1000); // every 5 mins

    // AEA search box listener
    $("#txtAEAfilter").keyup(function () {
        FilterAEA();
    });
    $('input[type=search]').on('search', function () {
        FilterAEA();
    });

    // add the event listeners
    addModelEvents(); // add model settings event handlers
    addNodeEvents(); // add node settings event handlers       
    addDragEvents(); // add drag drop event handlers
    addModelButtonEvents(); // add button click events for icons at top of chart
    addUndoEvents();
    // Go get all the data
    GetModelParms();
})

function AndFinally() {
    // The last routines called when everything else is complete..  called from the end of getmodelparms complete
    iawVerbose("AndFinally()");

    // initialise the html editors (uses MGLB so has to be done after getmodelparms)
    InitialiseControls();

    // Attach the handler to the panel resizing tool
    $ID("dragMe").addEventListener('mousedown', PanelsMouseDownHandler);

    $ID("myDiagramDiv").addEventListener("mousedown", e => {
        //iawVerbose("myDiagramDiv.mousedown",e.button);
        if (e.button == 0 || e.button == 2)
            SuppressSelectionMove = true;
    });
    $ID("myDiagramDiv").addEventListener("mouseup", e => {
        //iawVerbose("myDiagramDiv.mouseup", e.button);
        if (e.button == 0 || e.button == 2)
            SuppressSelectionMove = false;
    });

    if (MGLB.model_editable == true) {
        myDiagram.isModelReadOnly = false;

    } else {
        myDiagram.isModelReadOnly = true;
        myDiagram.allowDelete = false;
        myDiagram.allowMove = false;
        myDiagram.allowDragOut = false;
        myDiagram.allowDrag = false;
        myDiagram.allowTextEdit = false;
    }

    // show the screen..  have to do this before the vertical div resizing else it doesn't work
    $ID("divOuter").style.display = 'block';

    // set the size of the page divs so they reach the bottom of the page, but no more (avoid page v-scrollbar)
    ResizeDiv("divOuter");
    ResizeDiv("divUnallocated");
    ResizeDiv("myDiagramDiv");
    window.addEventListener('resize', function (event) {
        ResizeDiv("divOuter");
        ResizeDiv("divUnallocated");
        ResizeDiv("myDiagramDiv");
    });
    BuildAEA();

    if (ChartOrientation(LeftToRight, RightToLeft))
        ZoomToFit();
}

class SideTreeLayout extends go.TreeLayout {
    makeNetwork(coll) {
        const net = super.makeNetwork(coll);
        // copy the collection of TreeVertexes, because we will modify the network
        const vertexcoll = new go.Set( /*go.TreeVertex*/);
        vertexcoll.addAll(net.vertexes);
        for (const it = vertexcoll.iterator; it.next();) {
            const parent = it.value;
            // count the number of assistants
            let acount = 0;
            const ait = parent.destinationVertexes;
            while (ait.next()) {
                if (isAssistant(ait.value.node)) acount++;
            }
            // if a vertex has some number of children that should be assistants
            if (acount > 0) {
                // remember the assistant edges and the regular child edges
                const asstedges = new go.Set( /*go.TreeEdge*/);
                const childedges = new go.Set( /*go.TreeEdge*/);
                let eit = parent.destinationEdges;
                while (eit.next()) {
                    const e = eit.value;
                    if (isAssistant(e.toVertex.node)) {
                        asstedges.add(e);
                    } else {
                        childedges.add(e);
                    }
                }
                // first remove all edges from PARENT
                eit = asstedges.iterator;
                while (eit.next()) {
                    parent.deleteDestinationEdge(eit.value);
                }
                eit = childedges.iterator;
                while (eit.next()) {
                    parent.deleteDestinationEdge(eit.value);
                }
                // if the number of assistants is odd, add a dummy assistant, to make the count even
                if (acount % 2 == 1) {
                    const dummy = net.createVertex();
                    net.addVertex(dummy);
                    net.linkVertexes(parent, dummy, asstedges.first().link);
                }
                // now PARENT should get all of the assistant children
                eit = asstedges.iterator;
                while (eit.next()) {
                    parent.addDestinationEdge(eit.value);
                }
                // create substitute vertex to be new parent of all regular children
                const subst = net.createVertex();
                net.addVertex(subst);
                // reparent regular children to the new substitute vertex
                eit = childedges.iterator;
                while (eit.next()) {
                    const ce = eit.value;
                    ce.fromVertex = subst;
                    subst.addDestinationEdge(ce);
                }
                // finally can add substitute vertex as the final odd child,
                // to be positioned at the end of the PARENT's immediate subtree.
                const newedge = net.linkVertexes(parent, subst, null);
            }
        }
        return net;
    };
    assignTreeVertexValues(v) {
        // if a vertex has any assistants, use Bus alignment
        let any = false;
        const children = v.children;
        for (let i = 0; i < children.length; i++) {
            const c = children[i];
            if (isAssistant(c.node)) {
                any = true;
                break;
            }
        }
        if (any) {
            // this is the parent for the assistant(s)
            v.alignment = go.TreeLayout.AlignmentBus; // this is required
            v.nodeSpacing = 50; // control the distance of the assistants from the parent's main links
            //v.margin = new Margin(0,0,0,0);
            // v.layerSpacing = 0;
            // v.rowSpacing = 15;
        } else if (v.node == null && v.childrenCount > 0) {
            v.layerSpacing = 0;
        }
    };
    commitLinks() {
        super.commitLinks();
        // make sure the middle segment of an orthogonal link does not cross over the assistant subtree
        const eit = this.network.edges.iterator;
        while (eit.next()) {
            const e = eit.value;
            if (e.link == null) continue;
            const r = e.link;
            // does this edge come from a substitute parent vertex?
            const subst = e.fromVertex;
            if (subst.node == null && r.routing == go.Link.Orthogonal) {
                r.updateRoute();
                r.startRoute();
                // middle segment goes from point 2 to point 3
                const p1 = subst.center; // assume artificial vertex has zero size
                const p2 = r.getPoint(2).copy();
                const p3 = r.getPoint(3).copy();
                const p5 = r.getPoint(r.pointsCount - 1);
                let dist = 10;
                if (subst.angle == 270 || subst.angle == 180) dist = -20;
                if (subst.angle == 90 || subst.angle == 270) {
                    p2.y = p5.y - dist; // (p1.y+p5.y)/2;
                    p3.y = p5.y - dist; // (p1.y+p5.y)/2;
                } else {
                    p2.x = p5.x - dist; // (p1.x+p5.x)/2;
                    p3.x = p5.x - dist; // (p1.x+p5.x)/2;
                }
                r.setPoint(2, p2);
                r.setPoint(3, p3);
                r.commitRoute();
            }
        }
    }
}

function init(data) {
    iawVerbose("init()");

    SetChartBackground();

    go.Diagram.licenseKey = $ID("hdnGoJsKey").value;

    // Since 2.2 you can also author concise templates with method chaining instead of GraphObject.make
    // For details, see https://gojs.net/latest/intro/buildingObjects.html
    const $ = go.GraphObject.make; // for conciseness in defining templates
    myDiagram = $(go.Diagram, "myDiagramDiv", {
        // have mouse wheel events zoom in and out instead of scroll up and down
        maxSelectionCount: 1, // users can select only one part at a time        
        allowDragOut: false, //true,
        scrollMode: go.Diagram.InfiniteScroll,
        initialDocumentSpot: go.Spot.Top,
        initialViewportSpot: go.Spot.Top,
        initialContentAlignment: go.Spot.TopCenter,
        //initialAutoScale: go.Diagram.Uniform,    

        layout: $(SideTreeLayout, { // alt refers to sibling nodes that are layed out using the busstop method
            sorting: go.TreeLayout.SortingAscending,
            comparer: function (va, vb) {
                if (!va.node || !vb.node) return 0;

                var da = va.node.data;
                var db = vb.node.data;

                if (ChartOrientation(TopDown, BottomUp)) {
                    if (da.sequence < db.sequence) return -1;
                    if (da.sequence > db.sequence) return 1;
                } else {
                    if (da.sequence > db.sequence) return -1;
                    if (da.sequence < db.sequence) return 1;
                }

                return 0;
            },
            alternateSorting: go.TreeLayout.SortingAscending,
            alternateComparer: function (va, vb) {
                if (!va.node || !vb.node) return 0;

                var da = va.node.data;
                var db = vb.node.data;

                if (ChartOrientation(TopDown, BottomUp)) {
                    if (da.sequence < db.sequence) return -1;
                    if (da.sequence > db.sequence) return 1;
                } else {
                    if (da.sequence > db.sequence) return -1;
                    if (da.sequence < db.sequence) return 1;
                }

                return 0;
            },
            treeStyle: go.TreeLayout.StyleLastParents,

            //arrangement: go.TreeLayout.ArrangementHorizontal, // how the trees are layed out
            arrangement: go.TreeLayout.ArrangementFixedRoots,

            angle: MGLB.chartDirection, // make tree grown down
            layerSpacing: 33, // distance between parent and children
            nodeSpacing: 25, // horizontal distance between sibling nodes
            rowSpacing: 25, // vertical distance between siblings 
            alternateAlignment: go.TreeLayout.AlignmentBus, // alt siblings use busstop method
            alternateAngle: MGLB.chartDirection, // make alt tree grown down
            alternateLayerSpacing: 30, // alt vertical distnace between sibs
            alternateNodeSpacing: 22, // alt horizontal distance between sibling nodes
            alternateRowSpacing: 33,
            alternatePortSpot: new go.Spot(0, 0.999, 20, 0),
            alternateChildPortSpot: go.Spot.Left,

            boundsComputation: (part, lay) => {
                const b = part.actualBounds.copy();
                if (!part.data.isGroup) {
                    b.height = part.data.node_height;
                    if (ChartOrientation(TopDown, BottomUp))
                        b.height += 20;
                }
                return b;
            }

        }),

        "dragSelectingTool.isEnabled": false,
        "toolManager.mouseWheelBehavior": go.ToolManager.WheelZoom,
        "commandHandler.archetypeGroupData": {
            isGroup: true
        },
        "ModelChanged": e => {
            //iawVerbose("ModelChanged:");      // commented out due to being called a lot!
        },
        "ExternalObjectsDropped": e => {
            // Can't find when this is called
            iawVerbose("ExternalObjectsDropped:");
            var selnode = e.diagram.selection.first();
            if (selnode != null) {
                var getimage = GetNodePicture(selnode.data.item_ref, MGLB.node_height);

                myDiagram.startTransaction("SetNodePicture");

                selnode.data.node_bg = data.node_bg;
                selnode.data.source = getimage.node_picture;
                selnode.data.visible = true;

                e.diagram.removeParts(e.diagram.selection);
                e.diagram.model.addNodeData(selnode.data);

                myDiagram.commitTransaction("SetNodePicture");
            }
        },
        "clickCreatingTool.insertPart": function (loc) {
            iawVerbose("clickCreatingTool.insertPart:");

            const node = go.ClickCreatingTool.prototype.insertPart.call(this, loc);

            if (node !== null) {

                this.diagram.select(node);
                this.diagram.commandHandler.scrollToPart(node);
                this.diagram.commandHandler.editTextBlock(node.findObject("NAMETB"));
            }
            return node;
        },
        "commandHandler.deleteSelection": function () {
            iawVerbose("commandHandler.deleteSelection:");

            var delNode = myDiagram.selection.first();

            /* Deleting the selected node from the diagram
                    
            If it is a root node and the node_type is "Detail", "Vacant" or "Team" then we can't delete if 
                it has children as there would be nowhere for the children to go.  If however, there is only one 
                child, then you can delete the node as the child could then become the new root node.

            For "Detail" nodes, when we delete them, they get added into the AEA

            We should always be able to delete label 'note' nodes

            Node type "ParentGroup"
                If there is only 1 CoParent then you can delete the node and the CoParent replaces the ParentGroup
                    Any Children of the ParentGroup will become children of the replacement node

                If there are multiple CoParent nodes then you may only delete the ParentGroup if it doesn't have any children.
                    The CoParents will become children of the parent of the ParentGroup.

                // count the number of CoParent nodes
                var nodeCount = group.findSubGraphParts().count();
            */

            if (delNode !== null) {
                var ChildCount = 0;
                var ParentNode = null;

                if (delNode.data.isCoParent == true) {

                    ParentNode = myDiagram.findNodeForKey(delNode.data.parent);
                    ChildCount = getSubNodes(ParentNode).count;
                    if (ChildCount == 1) {
                        return;
                    }
                } else {
                    ChildCount = delNode.findTreeChildrenNodes().count;
                    ParentNode = delNode.findTreeParentNode();
                }

                if (delNode.data.isGroup == true && delNode.data.node_type == 'ParentGroup') {
                    myDiagram.startTransaction("parent node remove");
                    var subNodes = [];
                    // Find out sub nodes only 
                    for (var mit = delNode.memberParts; mit.next();) {
                        var part = mit.value; // part is now a Part within the Group
                        if (part instanceof go.Node) {
                            subNodes.push(part);
                        }
                    }

                    // if empty parent groups found delete it
                    if (subNodes.count == 0 && ChildCount == 0) {
                        myDiagram.model.removeNodeData(delNode.data);
                        myDiagram.removeParts(delNode.findLinksInto());
                    }

                    // if multiple co-parent exists then return it 
                    if (subNodes.length > 1) {
                        return;
                    }
                    // Only one subnodes should exists
                    var subNode = subNodes[0].part;
                    if (subNode != null) {
                        var NextSeq = 0;
                        var parentKey = 0;
                        if (ParentNode != null) {
                            //NextSeq = ResequenceChildren(ParentNode);
                            NextSeq = delNode.data.sequence;
                            parentKey = ParentNode.data.key;
                        } else {
                            NextSeq = delNode.data.sequence;
                            parentKey = delNode.data.parent;
                        }

                        // This section add for setting line data
                        if (subNode.data.node_type != 'Vacant' && subNode.data.node_type != 'Team') {
                            prepareModelDataItemForType('Detail', subNode, MGLB.node_height);
                        }

                        SetProperty(subNode.data, "parent", parentKey);
                        SetProperty(subNode.data, "sequence", NextSeq);
                        SetProperty(subNode.data, "group", '');
                        if (subNode.data.isCoParent == true) {
                            SetProperty(subNode.data, "isCoParent", false);
                            SetProperty(subNode.data, "isAssistant", delNode.data.isAssistant);
                        }
                        NextSeq = 0;
                        NextSeq = ResequenceChildren(subNode);

                        // attach any child nodes to the node's parent
                        var child = delNode.findTreeChildrenNodes();
                        while (child.next()) {
                            SetProperty(child.value.data, "parent", subNode.data.key);
                            SetProperty(child.value.data, "sequence", NextSeq);
                            NextSeq++;
                        }
                        var linksIn = delNode.findLinksInto();
                        if (linksIn.count > 0) {
                            while (linksIn.next()) {
                                var linkIn = linksIn.value;
                                addModelLink(linkIn.data.from, subNode.data.key);
                            }
                        }

                        var linksOut = delNode.findLinksOutOf();
                        if (linksOut.count > 0) {
                            while (linksOut.next()) {
                                var linkOut = linksOut.value;
                                addModelLink(subNode.data.key, linkOut.data.to);
                                //myDiagram.model.addLinkData({ from: subNode.data.key, to: linkOut.data.to });
                            }

                        }

                        myDiagram.removeParts(delNode.findLinksInto());
                        myDiagram.removeParts(delNode.findLinksOutOf());
                        myDiagram.model.removeNodeData(delNode.data);

                    }
                    myDiagram.commitTransaction("parent node remove");
                } else {
                    // may not delete root node of it has more than one child
                    if (ParentNode == null && ChildCount > 1) return;

                    // if detail node and we can delete it, then add the node back to the AEA
                    if (delNode.data.node_type == "Detail") {
                        GetUnallocatedDetail(delNode.data.item_ref);
                        BuildAEA();
                    }

                    if (ParentNode == null && ChildCount < 2) {
                        myDiagram.startTransaction("node remove");
                        if (delNode.data.isCoParent) {
                            var parentGroupNode = myDiagram.findNodeForKey(delNode.data.parent);
                            if (parentGroupNode.memberParts.count == 1) {
                                return;
                            } else {
                                myDiagram.model.removeNodeData(delNode.data);
                            }
                        } else {
                            // deleting the root node
                            var childNode = delNode.findTreeChildrenNodes().first();
                            if (childNode) {
                                myDiagram.removeParts(childNode.findLinksInto(), true);
                                SetProperty(childNode.data, "parent", 0);
                            }

                            // Remove the root node from the diagram
                            myDiagram.model.removeNodeData(delNode.data);
                        }

                        myDiagram.commitTransaction("node remove");

                    } else {
                        if (ParentNode != null) {
                            if (delNode.data.isCoParent == false) {
                                var NextSeq = 0;
                                NextSeq = ResequenceChildren(ParentNode);
                                // attach any child nodes to the node's parent
                                var child = delNode.findTreeChildrenNodes();
                                while (child.next()) {
                                    SetProperty(child.value.data, "parent", ParentNode.data.key);
                                    SetProperty(child.value.data, "sequence", NextSeq);
                                    myDiagram.removeParts(child.value.findLinksInto());

                                    addModelLink(ParentNode.data.key, child.value.data.key);

                                    NextSeq++;
                                }
                                myDiagram.removeParts(delNode.findLinksInto());
                                myDiagram.model.removeNodeData(delNode.data);
                                resetSequenceTreeChildren(ParentNode);
                            } else {
                                myDiagram.startTransaction("node remove");
                                myDiagram.removeParts(delNode.findLinksInto());
                                myDiagram.model.removeNodeData(delNode.data);
                                //var colNumber = getColumnNumber(ParentNode.memberParts.count);
                                //SetProperty(ParentNode.data, "columns", colNumber);
                                SetProperty(ParentNode.data, "columns", ParentNode.memberParts.count);
                                myDiagram.commitTransaction("node remove");
                                resetSequenceSubGraph(ParentNode);
                            }
                        }
                    }
                }

                ModelChanged(true)
            }
        },
        "InitialLayoutCompleted": function (e) {
            iawVerbose("InitialLayoutCompleted:");
            myDiagram.nodes.each((node) => {
                if (node.data.node_type === "Label") {

                    // Ensure that the node dimensions are set to be equal to the larger of width or height
                    var maxDimension = Math.max(node.data.node_height, node.data.node_width);
                    var label_Shape = node.findObject("Label_Shape");
                    if (label_Shape) {
                        label_Shape.width = maxDimension;
                        label_Shape.height = maxDimension;
                    }
                    // Update visual appearance
                    node.updateTargetBindings();
                }
            });

            if (!e.diagram.nodes.all(function (n) {
                return n.location.isReal();
            })) {
                e.diagram.layoutDiagram(true);
            }
            $ID("btnCentreRootNode").style.display = "inline-block";
            $ID("btnCentreTop").style.display = "none";
            CenterToRootNode();
        },
        "LayoutCompleted": e => {
            iawVerbose("LayoutCompleted");
            if (!SuppressLayoutComplete) {
                // lay out any note nodes to the left of the root node
                var locX = 0;
                var locY = 0;
                var rootTop = 0;
                var rootRight = 0;
                var rootBottom = 0;
                var rootLeft = 0;
                var spacing;

                var rootNode = GetDetailRootNode();
                if (rootNode) {
                    rootTop = rootNode.actualBounds.top;
                    rootRight = rootNode.actualBounds.right;
                    rootBottom = rootNode.actualBounds.bottom;
                    rootLeft = rootNode.actualBounds.left;
                }

                SuppressLayoutComplete = true;
                myDiagram.startTransaction("move label nodes");
                //myDiagram.scrollMargin = 200;
                // set the initial positions for the first label
                switch (MGLB.chartDirection) {
                    case TopDown:
                        spacing = 10;
                        locX = rootLeft + 15;
                        break;
                    case RightToLeft:
                        spacing = 5;
                        locY = rootTop;
                        break;
                    case BottomUp:
                        spacing = 10;
                        locX = rootRight - 15;
                        break;
                    case LeftToRight:
                        spacing = 5;
                        locY = rootBottom;
                        break;
                }
                let cnt = 0;
                myDiagram.nodes.each(node => {
                    if (node.data.node_type === "Label") {
                        // Calculate new location

                        switch (MGLB.chartDirection) {
                            case TopDown:
                                locY = rootTop + 5;
                                locX = locX - node.actualBounds.width - spacing;
                                break;
                            case RightToLeft:
                                locX = rootRight - node.actualBounds.width / 2;
                                cnt == 0 ? locY = locY - node.actualBounds.height - 20 : locY = locY - node.actualBounds.height - spacing;
                                break;
                            case BottomUp:
                                locY = rootTop + 30;
                                locX = locX + node.actualBounds.width + spacing;
                                break;
                            case LeftToRight:
                                locX = rootLeft + node.actualBounds.width / 2;
                                cnt == 0 ? locY = locY + 20 : locY = locY + node.actualBounds.height + spacing;
                                break;
                        }

                        // Update moving node's location
                        node.location = new go.Point(locX, locY);
                        cnt++;
                    }
                });
                myDiagram.commitTransaction("move label nodes");
                SuppressLayoutComplete = false;
            }

            // ensure that nodes vertial alignment is top or center based on the height of the table inside the node
            e.diagram.nodes.each(function (node) {
                if (node.data.node_type == 'Detail' || node.data.node_type == 'Team' || node.data.node_type == 'Vacant') {
                    let table = node.findObject("TABLE"); // assuming your table has the name "TABLE"
                    table.alignment = table.actualBounds.height > node.actualBounds.height ? go.Spot.Top : go.Spot.Center;
                }
            });
            e.diagram.nodes.each(function (node) {
                if (node instanceof go.Group) {
                    let rowWidths = new Map();
                    let minYRange = 10;

                    node.memberParts.each(function (part) {
                        if (part instanceof go.Node) {

                            // Replace this line
                            //let [centerX, centerY] = part.data.location.split(' ').map(Number);
                            // with this
                            let centerX = part.data.location.x;
                            let centerY = part.data.location.y;

                            let width = part.data.node_width;
                            let leftEdge = centerX - width / 2;
                            let rightEdge = centerX + width / 2;
                            let height = part.data.node_height;
                            let topY = centerY - height / 2;

                            let foundRow = false;
                            rowWidths.forEach((value, key) => {
                                if (Math.abs(key - topY) <= minYRange) {
                                    foundRow = true;
                                    let currentEdges = rowWidths.get(key);
                                    if (leftEdge < currentEdges[0]) currentEdges[0] = leftEdge;
                                    if (rightEdge > currentEdges[1]) currentEdges[1] = rightEdge;
                                    rowWidths.set(key, currentEdges);
                                }
                            });

                            if (!foundRow) {
                                rowWidths.set(topY, [leftEdge, rightEdge]);
                            }
                        }
                    });

                    let maxWidth = 0;
                    rowWidths.forEach((edges) => {
                        let rowWidth = edges[1] - edges[0];
                        if (rowWidth > maxWidth) maxWidth = rowWidth;
                    });

                    var textBlock = node.findObject("GroupTitle");
                    if (textBlock) {
                        textBlock.width = maxWidth - 6;
                    }
                }
            });
            e.diagram.layoutDiagram(true);
            // update the link colours
            //ApplyLinkColours();
        },
        "ChangedSelection": function (e) {
            iawVerbose("ChangedSelection:");

            var sel = e.diagram.selection.first();
            if (sel) {

                //if (sel.data.node_type == "Label") return;

                if (sel.data.node_type == "Label") {
                    if (MGLB.model_editable == false) {
                        myDiagram.clearSelection();
                        GetNodeInfoLabel(sel);
                    }
                    return;
                }

                if (SuppressSelectionMove == true) return;
                if (!e.diagram.viewportBounds.containsRect(sel.actualBounds)) {
                    e.diagram.centerRect(sel.actualBounds);
                }
            }
        },
        "SelectionMoved": function (e) {
            iawVerbose("SelectionMoved:");

            var sel = e.diagram.selection.first();
            if (sel) {
                myDiagram.currentTool.doCancel();
                myDiagram.layoutDiagram(true);
                return false;
            }

        },
        "commandHandler.doKeyDown": function () {
            iawVerbose("commandHandler.doKeyDown:");

            if (this.diagram.lastInput.key === 'Esc') {
                var myOverviewDiv = document.getElementById("myOverviewDiv");
                if (myOverviewDiv) myOverviewDiv.style.display = "none";
            }
            go.CommandHandler.prototype.doKeyDown.call(this);
        },
        "undoManager.isEnabled": true, // enable undo & redo
        "ViewportBoundsChanged": function () {
            //iawVerbose("ViewportBoundsChanged:");
            // If the diagram is zoomed in, display show magnifier icon
            if (myDiagram.scale >= 0.75)
                $ID("btnShowMagnifier").style.visibility = "hidden"
            else
                $ID("btnShowMagnifier").style.visibility = "visible"
        },
        "PartResized": function (e) {
            
            var sel = e.diagram.selection.first();
            if (sel == null) return;

            var obj = e.subject;
            var photo = "";
            var picture_width = 0;
            if (sel.data.node_type !== "Label") {
                if (obj.height < 50) obj.height = 50;
                if (obj.height > MGLB.max_node_height) obj.height = MGLB.max_node_height;
                if (obj.width < 100) obj.width = 100;
                if (obj.width > MGLB.max_node_width) obj.height = MGLB.max_node_width;

                var dataObj = myDiagram.model.findNodeDataForKey(sel.data.key);
                if (MGLB.show_photos == true) {
                    var getimage = GetNodePicture(sel.data.item_ref, obj.height)
                    photo = getimage.node_picture;
                    picture_width = getimage.picture_width;
                }
                setTimeout(() => {
                    SetProperty(dataObj, "source", photo);
                }, 100);
                myDiagram.layoutDiagram(true);
                ModelChanged(true);
                resizedNodeHeight = obj.height;
                resizedNodeWidth = obj.width;
                SetProperty(dataObj, "node_height", obj.height);
                SetProperty(dataObj, "node_width", obj.width);
                SetProperty(dataObj, "picture_width", picture_width);
                SetProperty(dataObj, "node_table_width",
                    CalcNodeTableWidth(dataObj.node_type,
                        dataObj.photoshow,
                        obj.width,
                        picture_width));

            } else {

                if (obj.height < 50) obj.height = 50;
                if (obj.height > 100) obj.height = 100;
                if (obj.width < 50) obj.width = 50;
                if (obj.width > 100) obj.width = 100;
                
                myDiagram.startTransaction("resize labels");
                //console.log("label width", obj.height)

                myDiagram.nodes.each((node) => {
                    if (node.data.node_type === "Label") {
                      var dataObj = myDiagram.model.findNodeDataForKey(node.data.key);

                        // Ensure that the node dimensions are set to be equal to the larger of width or height
                        var maxDimension = Math.max(obj.height, obj.width);
                      // Update the diagram and model for each node
                        SetProperty(dataObj, "node_height", maxDimension);
                        SetProperty(dataObj, "node_width", maxDimension);
                        // console.log("Label width: ", node.data.key, dataObj.node_width)
                        var label_Shape = node.findObject("Label_Shape");
                        if (label_Shape) {
                            label_Shape.width = maxDimension;
                            label_Shape.height = maxDimension;
                        }

                        // Update visual appearance
                      node.updateTargetBindings();
                    }
                  });
                myDiagram.commitTransaction("resize labels");
                myDiagram.layoutDiagram(true);
                ModelChanged(true);
            }
        },
    });

    myDiagram.animationManager.duration = 300; // milliseconds
    myDiagram.allowUndo = false;
    myDiagram.allowRedo = false;
    myDiagram.allowClipboard = false;

    // An array to hold the nodes being dragged
    var draggedNodes = [];

    myDiagram.toolManager.draggingTool.doActivate = function () {
        go.DraggingTool.prototype.doActivate.call(this);
        // Change the opacity of nodes and their group members when they are dragged
        myDiagram.selection.each(function (part) {
            if (part instanceof go.Node || part instanceof go.Group) {
                part.opacity = 0.5;
                if (part instanceof go.Group) {
                    part.memberParts.each(function (memberPart) {
                        memberPart.opacity = 0.5;
                    });
                }
            }
        });
    };

    myDiagram.toolManager.draggingTool.doDeactivate = function () {
        // first call the base method to stop the dragging operation
        go.DraggingTool.prototype.doDeactivate.call(this);

        // then restore the opacity of nodes and their group members after dragging
        myDiagram.selection.each(function (part) {
            if (part instanceof go.Node || part instanceof go.Group) {
                part.opacity = 1;
                if (part instanceof go.Group) {
                    part.memberParts.each(function (memberPart) {
                        memberPart.opacity = 1;
                    });
                }
            }
        });
    };

    var tool = myDiagram.toolManager.resizingTool;
    tool.isRealtime = false;

    tool.doMouseMove = function() {
        go.ResizingTool.prototype.doMouseMove.call(this);
        //console.log("this.adornedObject", this.adornedObject.part.data.node_type)
        if (this.adornedObject !== null) {
            var part = this.adornedObject.part;
            if (part instanceof go.Node && part.data.node_type === "Label") {
                var shape = part.findObject("Label_Shape");
                if (shape) {
                    if (shape.height < 50) shape.height = 50;
                    if (shape.height > 100) shape.height = 100;
                    if (shape.width < 50) shape.width = 50;
                    if (shape.width > 100) shape.width = 100;
                    var max = Math.max(shape.width, shape.height);
                    shape.width = max - 2;
                    shape.height = max - 2;
                    part.updateTargetBindings(); // To update data bindings if necessary
                }
            }
        }
    };
    
      tool.doMouseUp = function() {
        go.ResizingTool.prototype.doMouseUp.call(this);
        if (this.adornedObject !== null) {
          var part = this.adornedObject.part;
          if (part instanceof go.Node && part.data.node_type === "Label") {
                var shape = part.findObject("Label_Shape");
            if (shape) {
                    if (shape.height < 50) shape.height = 50;
                    if (shape.height > 200) shape.height = 200;
                    if (shape.width < 100) shape.width = 100;
                    if (shape.width > 200) shape.width = 200;
              var max = Math.max(shape.width, shape.height);
                    shape.width = max - 2;
                    shape.height = max - 2;
              part.updateTargetBindings(); // To update data bindings if necessary
            }
          }
        }
    }


    myDiagram.addDiagramListener("Modified", (e, node) => {
        iawVerbose("myDiagram.addDiagramListener.Modified:");

        if (MGLB.model_editable == true || actiontype == "") {
            temp = true;
            if (actiontype == "search") {
                myDiagram.isModified = false;
            }
        }
    });

    ModelChangedSearch(false);

    // override TreeLayout.commitNodes to also modify the background brush based on the tree depth level
    myDiagram.layout.commitNodes = function () {
        iawVerbose("myDiagram.layout.commitNodes");
        go.TreeLayout.prototype.commitNodes.call(myDiagram.layout);
        myDiagram.layout.network.vertexes.each(v => {
            if (v.node) {

                var color = v.node.data.node_border_fg;
                if (color == undefined || v.node.data.node_border_fg == MGLB.node_border_fg)
                    color = MGLB.node_border_fg;

                var shape = v.node.findObject("SHAPE");
                if (shape)
                    shape.stroke = color;
            }
        });
    };

    // this is used to determine feedback during drags
    function mayWorkFor(node1, node2) {
        iawVerbose("mayWorkFor()");

        if (!(node1 instanceof go.Node)) return false; // must be a Node
        if (node1 === node2) return false; // cannot work for yourself
        if (node2.isInTreeOf(node1)) return false; // cannot work for someone who works for you
        return true;
    }

    // This function provides a common style for most of the TextBlocks.
    // Some of these values may be overridden in a particular TextBlock.
    //function textStyle() {
    //    return {
    //        font: "9pt  Segoe UI,sans-serif",
    //        //stroke: data.node_fg
    //    };
    //}

    // This is the actual HTML context menu:
    var cxElement = $ID("contextMenu");
    // an HTMLInfo object is needed to invoke the code to set up the HTML cxElement
    var myContextMenu = $(go.HTMLInfo, {
        show: showContextMenu,
        hide: hideContextMenu
    });
    var myLabelToolTip = $(go.HTMLInfo, {
        show: showLabelToolTip,
        hide: LabelhideToolTip
    });
    // define the Node and Group templates
    myDiagram.nodeTemplateMap.add("",
        $(go.Node, "Spot", {
            toolTip: myLabelToolTip,
            isShadowed: true,
            shadowColor: "#ff0000",
            contextMenu: myContextMenu,
            resizable: MGLB.model_editable == true ? true : false,
            resizeObjectName: "NODE_SHAPE",
            locationSpot: go.Spot.TopCenter,
            selectionObjectName: "NODE_SHAPE",
            zOrder: 15,
            selectionAdornmentTemplate: $(go.Adornment, 'Auto',
            $(go.Shape, 'Rectangle', { fill: null, stroke: 'dodgerblue', strokeWidth: 2 }),
            $(go.Placeholder)
          ),
        },
            new go.Binding("location", "location").makeTwoWay(),
            new go.Binding("layerName", "isSelected", sel => sel ? "Foreground" : "").ofObject(),
            new go.Binding("isTreeExpanded").makeTwoWay(),
            new go.Binding("isShadowed", "", function (data) {
                return data.showShadow;
            }),
            new go.Binding("shadowColor", "", function (data) {
                return data.shadowColour;
            }),
            new go.Binding("selectable", "", function (data) {
                return MGLB.model_editable;
            }),
            new go.Binding("movable", "", function (data) {
                if (MGLB.model_editable == false)
                    return false;

                if (data.isCoParent == true) {
                    var previousGroup = myDiagram.findNodeForKey(data.group);
                    if (previousGroup != null) {
                        var countMem = getSubNodes(previousGroup);
                        if (countMem.count < 2) {
                            return false;
                        } else {
                            return true;
                        }
                    }
                } else {
                    if (data.parent == 0) return false; // can't move root
                    return true;
                }
            }), 
            {
            mouseDragEnter: (e, node) => {
                //  selnode is the node being dragged
                var selnode = node.diagram.selection.first(); // assume just one Node in selection

                // don't highlight if dragging a label node
                if (selnode.data.node_type == "Label") {
                    return;
                }
                if (selnode.data.dragtype == 'aea' && node.data.node_type != "Vacant") {
                    return;
                }
                if (selnode.data.node_type == "Team" && node.data.isCoParent == true) {
                    return;
                }
                if (node.data.node_type != "Vacant" && selnode.data.isCoParent == true && node.data.isCoParent == true) {
                    return;
                }
                if (selnode.data.node_type == "Detail" && node.data.isCoParent == true && node.data.node_type != "Vacant") {
                    return;
                }

                node.isHighlighted = true;
                if (node.data.group) {
                    var grp = myDiagram.findNodeForKey(node.data.group);
                    highlightGroup(e, grp, false);
                }
            },
            mouseDragLeave: (e, node) => {
                node.isHighlighted = false;
            },
            mouseDrop: (e, node) => {
                nt_mouseDrop(e, node);
            },
            click: function (e, obj) {
                linkOrder(obj.part); // send the node
            },

        },

            $(go.Panel, "Horizontal",
                $(go.Panel, "Auto",
                    nt_treeExpanderButton(),
                    new go.Binding("visible", "", () => {
                        return MGLB.chartDirection == RightToLeft;
                    }),
                ),
                $(go.Panel, "Vertical",
                    $(go.Panel, "Auto",
                        nt_treeExpanderButton(),
                        new go.Binding("visible", "", () => {
                            return MGLB.button_position == 'left' && MGLB.chartDirection == BottomUp;
                        }),
                    ),
                    $(go.Panel, "Spot",
                        nt_iconContent(),
                        new go.Binding("visible", "", () => {
                            return MGLB.chartDirection == BottomUp;
                        })
                    ),
                    nt_mainContent(),
                    $(go.Panel, "Horizontal",
                    {
                        alignment: go.Spot.Left,
                    },
                    nt_iconContent(),
                        new go.Binding("visible", "", () => {
                            return MGLB.chartDirection == TopDown;
                        })
                ),
                    $(go.Panel, "Auto",
                        nt_treeExpanderButton(),
                        new go.Binding("visible", "", () => {
                            return MGLB.button_position == 'left' && MGLB.chartDirection == TopDown;
                        }),
                    ),
                ),
                $(go.Panel, "Auto",
                    nt_treeExpanderButton(),
                    new go.Binding("visible", "", () => {
                        return MGLB.chartDirection == LeftToRight;
                    }),
                ),
            ),
        ) //Node 
    ); // Alternative node template

    myDiagram.nodeTemplateMap.add("Label",
        $(go.Node, "Auto", {
            toolTip: myLabelToolTip,
            isLayoutPositioned: false,
            movable: false,
            contextMenu: myContextMenu,
            resizable: true,
            resizeObjectName: "Label_Shape",
            selectionObjectName: "Label_Shape",
            click: (e, obj) => GetNodeInfoLabel(obj.part),
            resizeAdornmentTemplate: $(go.Adornment, "Spot",
                $(go.Placeholder),
                $(go.Shape, {
                    alignment: go.Spot.BottomRight,
                    cursor: "se-resize",
                    desiredSize: new go.Size(8, 8),
                    fill: "lightblue",
                    stroke: "dodgerblue"
                },)
            )
        },
          new go.Binding("visible", "", () => MGLB.button_position !== 'none'),
          new go.Binding("selectable", "", () => MGLB.model_editable),
            nodeStyle(),
            // $(go.Panel, 'Auto', { name: "Label_Content" },
            $(go.Shape, {
                name: "Label_Shape",
            },
                new go.Binding("figure", "node_corners"),
                new go.Binding("fill", "node_bg"),
                new go.Binding("stroke", "node_border_fg"),
                    // new go.Binding("width", "", data => { console.log("WWWWW", data.node_width); return data.node_width - 2}),
                    // new go.Binding("height", "", data => data.node_height - 2),
            ),
            $(go.Picture, {
                name: "Picture",
                margin: 0,
                alignment: go.Spot.Center,
            },
                // new go.Binding("desiredSize", "size", go.Size.parse).makeTwoWay(go.Size.stringify),
                new go.Binding("width", "node_width"),
                new go.Binding("height", "node_height"),
                new go.Binding("source", "", (obj, part) => {
                    const hexColor = rgbToHex(obj.part.data.node_fg);
                    loadSVGIcon(obj.part.data.label_icon, `${hexColor}`, obj.part.data.node_width, obj.part.data.node_height).then(url => {
                        part.source = url;
                    });
                    return "Icons/paperclip-solid.svg";
                }).ofObject(),
                    new go.Binding("scale", "", () => {
                        return 0.7;
                    })
                ),
            ),
        // ) // label node template
      );

    // define the parent group template
    myDiagram.groupTemplateMap.add("ParentGroup",
        $(go.Group, "Horizontal", {
            toolTip: myLabelToolTip,
            isSubGraphExpanded: true,
            selectionObjectName: "OBJSHAPE",
            locationSpot: go.Spot.TopCenter,
            mouseDragEnter: (e, grp, prev) => highlightGroup(e, grp, true),
            mouseDragLeave: (e, grp, next) => highlightGroup(e, grp, false),
            mouseDrop: finishDrop,
            handlesDragDropForMembers: true,
            computesBoundsAfterDrag: true,
            contextMenu: myContextMenu,
            zOrder: 14, // node is 15 so appears in front of the group
            selectionChanged: function (grp) {
                var lay = grp.isSelected ? "Foreground" : "";
                grp.layerName = lay;
                grp.findSubGraphParts().each(function (p) {
                    p.layerName = lay;
                });
            },
            click: function (e, obj) {
                linkOrder(obj.part);
            },
            selectionAdornmentTemplate: $(go.Adornment, 'Auto',
            $(go.Shape, 'Rectangle', { fill: null, stroke: 'dodgerblue', strokeWidth: 1 }),
            $(go.Placeholder)
          ),
        },
            new go.Binding("layout", "", function (data) {
                return makeLayout(data);
            }),
            new go.Binding("isTreeExpanded").makeTwoWay(),
            new go.Binding("isSubGraphExpanded").makeTwoWay(),
            new go.Binding("minSize", "", function (part) {
                return new size(MGLB.node_width, 0);
            }),
            new go.Binding("isShadowed", "", function (data) {
                return data.showShadow;
            }),
            new go.Binding("shadowColor", "", function (data) {
                return data.shadowColour;
            }),
            new go.Binding("selectable", "", function (data) {
                return MGLB.model_editable;
            }),
            $(go.Panel, "Auto",
                nt_treeExpanderButton(),
                new go.Binding("visible", "", () => {
                    return MGLB.chartDirection == RightToLeft;
                }),
            ),
            $(go.Panel, "Vertical",
                $(go.Panel, "Auto",
                    nt_treeExpanderButton(),
                    new go.Binding("visible", "", () => {
                        return MGLB.chartDirection == BottomUp;
                    })
                ),
            $(go.Panel, "Auto", {
                name: "GroupContent",
            },
                $(go.Shape, "Rectangle",
                    {
                        strokeWidth: 2,
                        name: "OBJSHAPE",
                        portId: "",
                    },
                    new go.Binding("figure", "node_corners"),
                    new go.Binding("stroke", "node_border_fg"),
                    new go.Binding("fill", "", function (part) {
                        return part.isHighlighted ? MGLB.node_highlight_bg : part.data.node_bg;
                    }).ofObject()),
                $(go.Panel, "Vertical", // Note the Vertical Panel
                    {
                        defaultAlignment: go.Spot.Center,
                        margin: new go.Margin(4, 4, 0, 4),
                    },
                    $(go.Panel, "Horizontal", // Note the Horizontal Panel
                        {
                            defaultAlignment: go.Spot.Center,
                        },
                        //new go.Binding("minSize", "node_width", function (w) {
                        //    return new go.Size(w - 20, NaN);
                        //}),
                        $("SubGraphExpanderButton", {
                            click: (e, obj) => {
                                var node = obj.part;
                                e.diagram.startTransaction("toggle expansion");
                                e.diagram.isModified = false;

                                if (node.data.isSubGraphExpanded == true) {
                                    SetProperty(node.data, "isSubGraphExpanded", false);
                                } else {
                                    SetProperty(node.data, "isSubGraphExpanded", true);
                                }

                                if (MGLB.model_editable == true) {
                                    ModelChanged(true);
                                } else {
                                    ModelChanged(false);
                                }
                                e.diagram.commitTransaction("toggle expansion");
                            },
                        },
                        new go.Binding("visible", "", () => {
                            return MGLB.button_position !== 'none' && (MGLB.model_editable === true || (MGLB.model_editable === false && MGLB.allow_drilldown === true));
                        }),
                    ),
                        $(go.Panel, "Auto", {
                            name: "GroupTitle",
                            background: "transparent",
                        },
                            $(go.Shape, "Rectangle", {
                                fill: null,
                                stroke: null,
                                stretch: go.GraphObject.Fill,
                            },
                                new go.Binding("fill", "", function (data) {
                                    if (data.node_text_bg_block) {
                                        return data.node_text_bg || 'transparent';
                                    } else return null;
                                })
                            ),
                            $(go.TextBlock, {
                                alignment: go.Spot.Left,
                                margin: new go.Margin(3, 10, 3, 10), // top, right, bottom, left 
                                editable: false,
                                isMultiline: false,
                                minSize: new go.Size(8, 10),
                                wrap: go.TextBlock.None,
                            },
                                new go.Binding("text", "line1").makeTwoWay(),
                                new go.Binding("alignment", "", function (data) {
                                    if (data.node_text_bg_block) return data.alignment1;
                                    else return go.Spot.Left;
                                }),
                                new go.Binding("isUnderline", "isUnderline1"),
                                new go.Binding("font", "font1"),
                                new go.Binding("background", "", function (data) {
                                    return data.node_text_bg || 'transparent';
                                }),
                                new go.Binding("stroke", "", function (part) {
                                    return part.isHighlighted ?
                                        MGLB.node_highlight_fg :
                                        part.data.fontColour1 != "" ?
                                            part.data.fontColour1 :
                                            part.data.node_fg;
                                }).ofObject(),
                            ),
                        ),

                        // image used for note (top right of group box)
                        //$(go.TextBlock, textStyle(), {
                        $(go.Picture, {
                                name: "GROUP_NOTE_ICON_PICTURE",
                                alignment: go.Spot.TopRight,
                                margin: new go.Margin(4, 0, 0, 4),
                                cursor: "pointer",
                                desiredSize: new go.Size(15, 15), 
                                mouseEnter: (e, obj) => {
                                    var data = obj.part.data;
                                    obj.stroke = data.node_icon_hover; // Change color to value in data
                                        const hexColor = rgbToHex(data.node_icon_hover);
                                            loadSVGIcon("Icons/paperclip-solid.svg", `${hexColor}`, 250, 250).then(url => {
                                                const iconImage = obj.part.findObject("GROUP_NOTE_ICON_PICTURE");
                                                if (iconImage) {
                                                    iconImage.source = url;
                                                }
                                            });
                                    },
                                mouseLeave: (e, obj) => {
                                    var data = obj.part.data;
                                    obj.stroke = data.node_icon_fg; // Change color to value in data
                                        const hexColor = rgbToHex(data.node_icon_fg);
                                            loadSVGIcon("Icons/paperclip-solid.svg", `${hexColor}`, 250, 250).then(url => {
                                                const iconImage = obj.part.findObject("GROUP_NOTE_ICON_PICTURE");
                                                if (iconImage) {
                                                    iconImage.source = url;
                                                }
                                            });
                                    },
                                click: (e) => {
                                    GetNodeInfoLabel(e.diagram.selection.first())
                                }
                            }, 
                                new go.Binding("visible", "", (data) => {
                                    return data.isNote && MGLB.button_position !== 'none';
                                }),
                                new go.Binding("source", "", (obj, part) =>      {
                                    if(obj.part.data.isNote) {
                                        const hexColor = rgbToHex(obj.part.data.node_icon_fg);
                                        loadSVGIcon("Icons/paperclip-solid.svg", `${hexColor}`, 250, 250).then(url => {
                                                part.source = url;
                                        });
                                    }
                                    return "Icons/paperclip-solid.svg";
                                }).ofObject(),
                                new go.Binding("stroke", "", function (part) {
                                    return part.isHighlighted ?
                                           MGLB.node_highlight_fg : part.data.node_icon_fg;
                            }).ofObject(),
                        ), // Note

                        //$(go.Shape, "Rectangle",  // Invisible placeholder
                        //    {
                        //        desiredSize: new go.Size(15, 15),  // Same size as the icon
                        //        fill: "transparent",  // Transparent fill
                        //        stroke: null,  // No border
                        //        visible: false  // Hidden by default
                        //    },
                        //    new go.Binding("visible", "isNote", (isNote) => !isNote)  // Show this when the icon is not visible
                        //)
                    ), // End of Horizontal Panel
                    $(go.Placeholder, // Placeholder moved here, to be below the title
                        {
                            padding: new go.Margin(5, 0, 5, 0),
                            margin: new go.Margin(0, 0, 0, 0),
                            minSize: new go.Size(1, 20),
                        }),
                    ),
                ),
                $(go.Panel, "Auto",
                    nt_treeExpanderButton(),
                    new go.Binding("visible", "", () => {
                        return MGLB.chartDirection == TopDown;
                    }),
                ),
            ),
            $(go.Panel, "Auto",
            nt_treeExpanderButton(),
                new go.Binding("visible", "", () => {
                    return MGLB.chartDirection == LeftToRight;
                }),
            ),
        )
    );

    // create the text object
    function nt_createTextBlock(lineNumber) {
        return $(go.Panel, "Auto",
            {
                row: lineNumber - 1,
                stretch: go.GraphObject.Horizontal,
                margin: new go.Margin(0, 0, 0, 0),
            },
            $(go.Shape, "Rectangle", // Background shape
                {
                    strokeWidth: 0,
                    fill: null,
                    stroke: null,
                    stretch: go.GraphObject.Fill,
                    margin: new go.Margin(0, 2, 0, 1),
                },
                new go.Binding("fill", "", function (data) {
                    if (data.node_text_bg_block)
                        return data[`fontBgColour${lineNumber}`] || data.node_text_bg || 'transparent';
                    return null;
                })
            ),
            $(go.TextBlock,
                {   
                    editable: false,
                    isMultiline: false,
                    minSize: new go.Size(8, 10),
                    wrap: go.TextBlock.None,
                    margin: new go.Margin(0, 2, 0, 2),
                },
                new go.Binding("text", `line${lineNumber}`).makeTwoWay(),
                new go.Binding("alignment", "", function (data) {
                    return data[`alignment${lineNumber}`];
                }),
                new go.Binding("isUnderline", `isUnderline${lineNumber}`),
                new go.Binding("font", `font${lineNumber}`),
                new go.Binding("background", "", function (data) {
                    var lineContent = data[`line${lineNumber}`];
                    if (lineContent === undefined || lineContent === null || lineContent.trim() === "")
                        return null;
                    if (!data.node_text_bg_block) {
                        return data[`fontBgColour${lineNumber}`] || data.node_text_bg || 'transparent';
                    }
                    return null;
                }),
                new go.Binding("stroke", "", function (part) {
                    // Determine the text color based on highlighting or data
                    return part.isHighlighted ? MGLB.node_highlight_fg :
                        part.data[`fontColour${lineNumber}`] != ""
                            ? part.data[`fontColour${lineNumber}`]
                            : part.data.node_fg;
                }).ofObject(),
                new go.Binding("visible", "", function (h, shape) {
                    return NodeLineVisibility(shape.part.data, lineNumber);
                })
            )
        );
    }
    function nt_mouseDrop(e, targetNode) {

        iawVerbose("mouseDrop - onto OCA node");
        // Something dropped onto an OCA node
        var diagram = targetNode.diagram;
        // dragNode is the node being dragged
        var dragNode = diagram.selection.first(); // assume just one Node in selection                

        if (dragNode.data.node_type == 'Vacant' && dragNode.data.isCoParent == true) {
            return false;
        }
        if (targetNode.data.isCoParent == true) {

            if (dragNode.data.isCoParent == true && targetNode.data.node_type != 'Vacant') {
                return;
            }

            if (dragNode.data.node_type == "Team" && targetNode.data.isCoParent == true) {
                return;
            }

            var parentGroupNode = myDiagram.findNodeForKey(targetNode.data.parent);
            if (parentGroupNode != null && parentGroupNode.data.parent == dragNode.data.key) {
                myDiagram.currentTool.doCancel();
                return;
            }
        }
        if (MGLB.model_editable == true && dragNode.data.isCoParent == true && targetNode.data.node_type != 'Vacant') {
            myDiagram.currentTool.doCancel();
            return false;
        } else if (MGLB.model_editable == true) {
            if (dragNode.data.node_type == "Label") {
                myDiagram.currentTool.doCancel();
                return;
            }

            if (mayWorkFor(dragNode, targetNode)) {

                ModelChanged(myDiagram.isModified);
                var link = dragNode.findTreeParentLink();
                if (targetNode.data.node_type == "Vacant" &&
                    (
                        (dragNode.data.dragtype == "oca" && targetNode.data.isCoParent == true && targetNode.data.parent == dragNode.data.parent) ||
                        (dragNode.data.dragtype == "oca" && dragNode.data.node_type != "Vacant" && targetNode.data.isCoParent == false) ||
                        (dragNode.data.dragtype == "aea")
                    )
                ) {

                    iawVerbose("Dragged onto a vacant node");
                    var thisNode = targetNode.data;
                    if (dragNode.data.dragtype == "aea") {
                        iawVerbose("From the AEA");
                        myDiagram.startTransaction("vacate");
                        // update the key, name, and comments

                        SetProperty(thisNode, "key", dragNode.data.key);
                        SetProperty(thisNode, "name", dragNode.data.name);
                        SetProperty(thisNode, "item_ref", dragNode.data.item_ref);

                        for (let i = 1; i <= 6; i++) {
                            SetProperty(thisNode, `line${i}`, dragNode.data[`line${i}`]);
                            SetProperty(thisNode, `font${i}`, dragNode.data[`font${i}`]);
                            SetProperty(thisNode, `isUnderline${i}`, dragNode.data[`isUnderline${i}`]);
                            SetProperty(thisNode, `fontColour${i}`, dragNode.data[`fontColour${i}`]);
                            SetProperty(thisNode, `fontBgColour${i}`, dragNode.data[`fontBgColour${i}`]);
                            SetProperty(thisNode, `alignment${i}`, dragNode.data[`alignment${i}`]);
                        }

                        SetProperty(thisNode, "dragtype", "oca");
                        SetProperty(thisNode, "isTreeExpanded", thisNode.isTreeExpanded == true ? true : false);
                        SetProperty(thisNode, "linkColour", dragNode.data.linkColour);
                        SetProperty(thisNode, "linkHover", dragNode.data.linkHover);
                        SetProperty(thisNode, "linkTooltip", dragNode.data.linkTooltip);
                        SetProperty(thisNode, "linkTooltipBackground", dragNode.data.linkTooltipBackground);
                        SetProperty(thisNode, "linkTooltipBorder", dragNode.data.linkTooltipBorder);
                        SetProperty(thisNode, "linkTooltipForeground", dragNode.data.linkTooltipForeground);
                        SetProperty(thisNode, "linkType", dragNode.data.linkType);
                        SetProperty(thisNode, "linkWidth", dragNode.data.linkWidth);
                        SetProperty(thisNode, "node_bg", dragNode.data.node_bg);
                        SetProperty(thisNode, "node_border_fg", dragNode.data.node_border_fg);
                        SetProperty(thisNode, "node_fg", dragNode.data.node_fg);
                        SetProperty(thisNode, "node_text_bg", dragNode.data.node_text_bg);
                        SetProperty(thisNode, "node_text_bg_block", dragNode.data.node_text_bg_block);
                        SetProperty(thisNode, "node_icon_fg", dragNode.data.node_icon_fg);
                        SetProperty(thisNode, "node_icon_hover", dragNode.data.node_icon_hover);
                        SetProperty(thisNode, "node_tt_bg", dragNode.data.node_tt_bg);
                        SetProperty(thisNode, "node_tt_border", dragNode.data.node_tt_border);
                        SetProperty(thisNode, "node_tt_fg", dragNode.data.node_tt_fg);
                        SetProperty(thisNode, "node_type", dragNode.data.node_type);
                        SetProperty(thisNode, "photoshow", MGLB.show_photos);
                        SetProperty(thisNode, "show_detail", dragNode.data.show_detail);
                        SetProperty(thisNode, "success", "false");
                        SetProperty(thisNode, "tooltip", dragNode.data.tooltip);
                        SetProperty(thisNode, "visible", dragNode.data.visible);

                        var getimage = GetNodePicture(thisNode.item_ref, parseInt(thisNode.node_height));
                        SetProperty(thisNode, "picture_width", getimage.picture_width);
                        SetProperty(thisNode, "node_table_width",
                            CalcNodeTableWidth('Detail', MGLB.show_photos, parseInt(MGLB.node_width), parseInt(getimage.picture_width)));

                        setTimeout(() => {
                            SetProperty(thisNode, "source", dragNode.data.source);
                        }, 100);

                        myDiagram.commitTransaction('vacate');

                        if (dragNode !== null) {

                            myDiagram.startTransaction("reparent remove");

                            if (thisNode.node_type == 'CoParent') {
                                // This will iterate over all links coming into the node and remove them
                                dragNode.findLinksInto().each(function (link) {
                                    myDiagram.remove(link);
                                });
                            }

                            const chl = dragNode.findTreeChildrenNodes();
                            // iterate through the children and set their parent key to our selected node's parent key
                            while (chl.next()) {
                                const emp = chl.value;
                                // myDiagram.model.setParentKeyForNodeData(emp.data, selnode.findTreeParentNode().data.key);
                                SetProperty(emp.data, "parent", dragNode.findTreeParentNode().data.key);
                                myDiagram.removeParts(emp.findLinksInto());
                                addModelLink(dragNode.findTreeParentNode().data.key, emp.data.key);
                                //myDiagram.model.addLinkData({ from: selnode.findTreeParentNode().data.key, to: emp.data.key });
                            }
                            // and now remove the selected node itself
                            myDiagram.model.removeNodeData(dragNode.data);
                            myDiagram.commitTransaction("reparent remove");
                        }
                        if (targetNode.data.isCoParent) {
                            prepareModelDataItemForType('CoParent', targetNode, MGLB.node_height);
                        }
                    } else {
                        iawVerbose("Dragged from", dragNode.data.dragtype);
                        if (targetNode.data.node_type == "Vacant") {
                            myDiagram.startTransaction("vacate");
                            // update the key, name, and comments
                            SetProperty(thisNode, "key", dragNode.data.key);
                            SetProperty(thisNode, "name", dragNode.data.name);
                            SetProperty(thisNode, "item_ref", dragNode.data.item_ref);

                            for (let i = 1; i <= 6; i++) {
                                SetProperty(thisNode, `line${i}`, dragNode.data[`line${i}`]);
                                SetProperty(thisNode, `font${i}`, dragNode.data[`font${i}`]);
                                SetProperty(thisNode, `isUnderline${i}`, dragNode.data[`isUnderline${i}`]);
                                SetProperty(thisNode, `fontColour${i}`, dragNode.data[`fontColour${i}`]);
                                SetProperty(thisNode, `fontBgColour${i}`, dragNode.data[`fontBgColour${i}`]);
                                SetProperty(thisNode, `alignment${i}`, dragNode.data[`alignment${i}`]);
                            }

                            SetProperty(thisNode, "isTreeExpanded", dragNode.tree_expanded);
                            SetProperty(thisNode, "linkColour", dragNode.data.linkColour);
                            SetProperty(thisNode, "linkHover", dragNode.data.linkHover);
                            SetProperty(thisNode, "linkTooltip", dragNode.data.linkTooltip);
                            SetProperty(thisNode, "linkTooltipBackground", dragNode.data.linkTooltipBackground);
                            SetProperty(thisNode, "linkTooltipBorder", dragNode.data.linkTooltipBorder);
                            SetProperty(thisNode, "linkTooltipForeground", dragNode.data.linkTooltipForeground);
                            SetProperty(thisNode, "linkType", dragNode.data.linkType);
                            SetProperty(thisNode, "linkWidth", dragNode.data.linkWidth);
                            SetProperty(thisNode, "node_bg", dragNode.data.node_bg);
                            SetProperty(thisNode, "node_border_fg", dragNode.data.node_border_fg);
                            SetProperty(thisNode, "node_fg", dragNode.data.node_fg);
                            SetProperty(thisNode, "node_text_bg", dragNode.data.node_text_bg);
                            SetProperty(thisNode, "node_text_bg_block", dragNode.data.node_text_bg_block);
                            SetProperty(thisNode, "node_icon_fg", dragNode.data.node_icon_fg);
                            SetProperty(thisNode, "node_icon_hover", dragNode.data.node_icon_hover);
                            SetProperty(thisNode, "node_tt_bg", dragNode.data.node_tt_bg);
                            SetProperty(thisNode, "node_tt_border", dragNode.data.node_tt_border);
                            SetProperty(thisNode, "node_tt_fg", dragNode.data.node_tt_fg);
                            SetProperty(thisNode, "node_type", dragNode.data.node_type);
                            SetProperty(thisNode, "photoshow", dragNode.data.photoshow);
                            SetProperty(thisNode, "show_detail", dragNode.data.show_detail);
                            SetProperty(thisNode, "success", "false");
                            SetProperty(thisNode, "tooltip", dragNode.data.tooltip);
                            SetProperty(thisNode, "visible", dragNode.data.visible);

                            var getimage = GetNodePicture(thisNode.item_ref, parseInt(thisNode.node_height));
                            SetProperty(thisNode, "picture_width", getimage.picture_width);
                            SetProperty(thisNode, "node_table_width",
                                CalcNodeTableWidth('Detail', MGLB.show_photos, parseInt(MGLB.node_width), parseInt(getimage.picture_width)));

                            setTimeout(() => {
                                SetProperty(thisNode, "source", dragNode.data.source);
                            }, 100);

                            if (thisNode.node_type == 'CoParent') {
                                // This will iterate over all links coming into the node and remove them
                                dragNode.findLinksInto().each(function (link) {
                                    myDiagram.remove(link);
                                });
                            }

                            var newParentKey = null;
                            if (targetNode.data.isCoParent) {
                                newParentKey = targetNode.data.parent;
                            } else {
                                newParentKey = targetNode.data.key;
                            }
                            var parentNode = myDiagram.findNodeForKey(newParentKey);
                            const chl = dragNode.findTreeChildrenNodes();
                            // iterate through the children and set their parent key to our selected node's parent key
                            while (chl.next()) {
                                const emp = chl.value;
                                //myDiagram.model.setParentKeyForNodeData(emp.data, node.data.key);
                                SetProperty(emp.data, "parent", parentNode.data.key);
                                myDiagram.removeParts(emp.findLinksInto());

                                addModelLink(parentNode.data.key, emp.data.key);
                                //myDiagram.model.addLinkData({ from: parentNode.data.key, to: emp.data.key });
                            }
                            myDiagram.model.removeNodeData(dragNode.data);

                            myDiagram.commitTransaction('vacate');
                            if (link !== null) {
                                myDiagram.removeParts(dragNode.findLinksInto());
                                link.fromNode = parentNode;
                            }

                            if (targetNode.data.isCoParent) {
                                //var colNumber = getColumnNumber(parentNode.memberParts.count);
                                SetProperty(parentNode.data, "columns", ParentNode.memberParts.count);
                                resetSequenceSubGraph(parentNode);
                            } else {
                                resetSequenceTreeChildren(parentNode);
                            }

                            if (targetNode.data.isCoParent) {
                                prepareModelDataItemForType('CoParent', targetNode, MGLB.node_height);
                            }
                        } else {
                            if (link !== null) { // reconnect any existing link

                                link.fromNode = targetNode;
                            } else {
                                if (dragNode != null) {
                                    var photo = "";
                                    if (MGLB.show_photos == false || MGLB.show_photos == "false") {
                                        photo = "";
                                    } else {
                                        var getimage = GetNodePicture(dragNode.data.item_ref, MGLB.node_height);
                                        photo = getimage.node_picture;
                                    }
                                    dragNode.data.node_bg = MGLB.node_bg;
                                    dragNode.data.source = photo;
                                    dragNode.data.visible = true;

                                    e.diagram.removeParts(e.diagram.selection);
                                    e.diagram.model.addNodeData(dragNode.data);
                                }
                                addModelLink(targetNode.data.key, dragNode.data.key);
                            }
                        }
                    }
                } else {
                    if (link !== null) { // reconnect any existing link
                        if (targetNode.data.isCoParent) {

                            if (targetNode.data.parent != null && dragNode.data.parent == targetNode.data.parent &&
                                (dragNode.data.node_type == 'Detail' || dragNode.data.node_type == 'Vacant' || dragNode.data.node_type == 'Detail')) {
                                var parentGroupNode = myDiagram.findNodeForKey(targetNode.data.parent);
                                // prepareCoParentNode(parentGroupNode, selnode);

                            } else if (targetNode.data.parent != null && dragNode.data.parent != targetNode.data.parent &&
                                dragNode.data.node_type == 'Team') {
                                var parentGroupNode = myDiagram.findNodeForKey(targetNode.data.parent);
                                if (parentGroupNode != null) {
                                    prepareChildNode(parentGroupNode, dragNode);
                                }

                            }

                        } else {
                            link.fromNode = targetNode;
                            if (targetNode.isTreeExpanded === false)
                                targetNode.isTreeExpanded = true;
                            if (targetNode) {
                                var previousParentKey = dragNode.data.parent;
                                var previousParentNode = myDiagram.findNodeForKey(previousParentKey);
                                SetProperty(dragNode.data, "parent", targetNode.data.key);
                                resetSequenceTreeChildren(targetNode);
                                resetSequenceTreeChildren(previousParentNode);
                            }
                        }

                    } else {
                        if (dragNode != null) {
                            if (targetNode.isTreeExpanded === false)
                                targetNode.isTreeExpanded = true;
                            var photo = "";
                            if (MGLB.show_photos == false || MGLB.show_photos == "false") {
                                photo = "";
                            } else {
                                var getimage = GetNodePicture(dragNode.data.item_ref, MGLB.node_height);
                                photo = getimage.node_picture;
                            }

                            dragNode.data.node_bg = MGLB.node_bg;
                            dragNode.data.source = photo;
                            dragNode.data.visible = true;

                            if (targetNode.data.isCoParent == true) {
                                var NextSeq = 0;
                                var parentNode = myDiagram.findNodeForKey(targetNode.data.parent);
                                NextSeq = ResequenceChildren(parentNode);
                                dragNode.data.parent = targetNode.data.parent;
                                //selnode.data.sequence = NextSeq;
                                if (dragNode.data.dragtype == 'aea') {
                                    dragNode.data.group = undefined;
                                    dragNode.data.isCoParent = false;
                                    dragNode.data.isAssistant = false;
                                    dragNode.data.sequence = NextSeq;
                                } else {
                                    dragNode.data.group = targetNode.data.parent;
                                    dragNode.data.isAssistant = false;
                                    dragNode.data.isCoParent = true;
                                    dragNode.data.sequence = NextSeq;
                                }

                            } else {
                                var NextSeq = 0;
                                var parentNode = myDiagram.findNodeForKey(targetNode.data.key);
                                NextSeq = ResequenceTheLastChildren(parentNode);
                                dragNode.data.parent = targetNode.data.key;
                                dragNode.data.sequence = NextSeq;
                            }

                            e.diagram.removeParts(e.diagram.selection);
                            e.diagram.model.addNodeData(dragNode.data);

                            if (targetNode.data.isCoParent == false) {
                                addModelLink(targetNode.data.key, dragNode.data.key);
                                SetProperty(dragNode.data, "dragtype", 'oca');
                            }
                            if (targetNode.data.isCoParent == true && dragNode.data.dragtype == 'aea') {
                                addModelLink(targetNode.data.parent, dragNode.data.key);
                                SetProperty(dragNode.data, "dragtype", 'oca');
                            }
                        }
                    }
                }
            }
        }
    }
    function nt_treeExpanderButton() {
        return !MGLB.allow_drilldown && !MGLB.model_editable ? {} :
            $(go.Panel, "Spot", {
                name: "treeBtnExpander",
                isClipping: false,
                toolTip: new go.Adornment()
            },
            new go.Binding("visible", "", () => {
                return MGLB.button_position !== 'none';
            }),
                $(go.Shape, {
                    alignment: go.Spot.Center,
                    fill: null,
                    stroke: null,
                    maxSize: new go.Size(0, 0),
                    margin: new go.Margin(0, 0, 0, 0),
                }),
                // Tree Expander Button
                $("TreeExpanderButton", {
                    alignment: go.Spot.TopLeft,
                    alignmentFocus: go.Spot.Top,
                    "ButtonIcon.strokeWidth": 0,
                    margin: new go.Margin(0, 0, 0, 0), // top, right, bottom, left
                    visible: true,
                },
                    new go.Binding("margin", "", (data) => {
                        switch (MGLB.chartDirection) {
                            case TopDown:
                                return MGLB.button_position == "bottomleft" ?
                                    new go.Margin(2, 2, 0, 0) :
                                    new go.Margin(2, 0, 0, 2)
                            case BottomUp:
                                return MGLB.button_position == "bottomleft" ?
                                    new go.Margin(0, 2, 2, 0) :
                                    new go.Margin(0, 0, 2, 2)
                            case RightToLeft:
                                return MGLB.button_position == "bottomleft" ?
                                    new go.Margin(0, 2, 0, 0) :
                                    new go.Margin(0, 2, 0, 2)
                            case LeftToRight:
                                return MGLB.button_position == "bottomleft" ?
                                    new go.Margin(0, 2, 0, 2) :
                                    new go.Margin(0, 0, 0, 2)
                        }

                    }),

                    new go.Binding("ButtonBorder.figure", "", () => {
                        return MGLB.button_shape;
                    }),
                    new go.Binding("ButtonBorder.fill", "", () => {
                        return MGLB.button_back_colour;
                    }),
                    new go.Binding("ButtonBorder.stroke", "", () => {
                        return MGLB.button_border_colour;
                    }),
                    new go.Binding("ButtonIcon.stroke", "", () => {
                        return MGLB.button_text_colour;
                    }),
                    $(go.Panel, "Horizontal", {
                        alignment: go.Spot.Center,
                        margin: new go.Margin(0, 4, 0, 4),
                    },
                        $(go.Panel, {},
                            $(go.Shape,  // the icon
                                {
                                    name: 'ButtonIcon1',
                                    figure: 'MinusLine',  // default value for isTreeExpanded is true
                                    stroke: '#424242',
                                    strokeWidth: 2,
                                    desiredSize: new go.Size(8, 8)
                                },
                                new go.Binding("stroke", "", () => {
                                    return MGLB.button_text_colour;
                                }),
                                // bind the Shape.figure to the Node.isTreeExpanded value using this converter:
                                new go.Binding('figure', 'isTreeExpanded',
                                    function (exp, shape) {
                                        return exp ? "MinusLine" : "PlusLine";
                                    }
                                ).ofObject()
                            ),
                        ),
                        $(go.TextBlock,
                            {
                                name: "NUM_Child",
                            },
                            new go.Binding("stroke", "", () => {
                                return MGLB.button_text_colour;
                            }),
                            new go.Binding("font", "", () => {
                                return MGLB.button_font;
                            }),
                            new go.Binding("text", "", (data, obj) => {
                                if (!data.isTreeExpanded) {
                                    return "(" + countAllDescendants(obj.part) + ")";
                                }
                            }).ofObject(),
                        ),
                    ),
                    {
                        mouseEnter: function (e, btn, prev) {
                            if (!btn.isEnabledObject()) return;
                            btn.findObject('ButtonIcon1').stroke = MGLB.button_text_hover;
                            var shape1 = btn.findObject('ButtonBorder');
                            shape1.fill = MGLB.button_back_hover;
                            shape1.stroke = MGLB.button_border_hover;
                            btn.findObject("NUM_Child").stroke = MGLB.button_text_hover;
                        },

                        mouseLeave: function (e, btn, prev) {
                            if (!btn.isEnabledObject()) return;
                            btn.findObject('ButtonIcon1').stroke = MGLB.button_text_colour
                            var shape = btn.findObject('ButtonBorder');
                            shape.fill = MGLB.button_back_colour;
                            shape.stroke = MGLB.button_border_colour;
                            btn.findObject("NUM_Child").stroke = MGLB.button_text_colour;
                        },

                        click: (e, obj) => {
                            var node = obj.part;
                            //const offset = node ? node.position.copy() : null; // node arg is optional
                            //const oldPos = myDiagram.position.copy();

                            e.diagram.startTransaction("toggle expansion");
                            e.diagram.isModified = false;
                            SetProperty(node.data, "isTreeExpanded", !node.data.isTreeExpanded);
                            ModelChanged(MGLB.model_editable);
                            e.diagram.commitTransaction("toggle expansion");

                            //if (offset) {
                            //    // shift the Diagram by the offset of the Node's current position and the old position
                            //    offset.subtract(node.position);
                            //    myDiagram.position = new go.Point(oldPos.x - offset.x, oldPos.y - offset.y)
                            //}
                        },
                    }) // end of tree expander
            );
    }
    function nt_treeButton(ButtonType) {
        // button type = "Note" || "Detail"

        return $(go.Panel, "Spot", {
            name: "treeBtn" + ButtonType,
            isClipping: false,
            toolTip: new go.Adornment()
        },
            $(go.Shape, {
                alignment: go.Spot.Center,
                fill: null,
                stroke: null,
                maxSize: new go.Size(0, 0),
                margin: new go.Margin(0, 0, 0, 0),
            }),
            $("Button", {
                margin: new go.Margin(0, 0, 0, 0),
                alignment: go.Spot.TopLeft,
                click: (e, obj) => {
                    if (ButtonType == "Note")
                        GetNodeInfoLabel(obj.part);
                    else
                        GetNodeInfo(obj.part);
                },
                mouseEnter: (e, obj) => {
                    var btn = obj.panel;
                    if (btn) {
                        btn.findObject("ButtonBorder").stroke = MGLB.button_border_hover;
                        btn.findObject("ButtonBorder").fill = MGLB.button_back_hover;
                        btn.findObject("BTN_TEXT").stroke = MGLB.button_text_hover;
                    }
                },
                mouseLeave: (e, obj) => {
                    var btn = obj.panel;
                    if (btn) {
                        btn.findObject("ButtonBorder").stroke = MGLB.button_border_colour;
                        btn.findObject("ButtonBorder").fill = MGLB.button_back_colour;
                        btn.findObject("BTN_TEXT").stroke = MGLB.button_text_colour;
                    }
                },
            },
                new go.Binding("visible", "", (data) => {
                    let button_position = get_btnPosition();
                    if (button_position == "left") return false;
                    return ButtonType == "Note" ?
                        data.isNote : data.show_detail;
                }),
                new go.Binding("margin", "", (data) => {
                    switch (MGLB.chartDirection) {
                        case TopDown:
                    return MGLB.button_position == "bottomleft" ?
                        new go.Margin(2, 2, 0, 0) :
                        new go.Margin(2, 0, 0, 2)
                        case BottomUp:
                            return MGLB.button_position == "bottomleft" ?
                                new go.Margin(0, 2, 2, 0) :
                                new go.Margin(0, 0, 2, 2)
                        default:
                            break;
                    }
                }),

                new go.Binding("ButtonBorder.figure", "", () => MGLB.button_shape),
                new go.Binding("ButtonBorder.stroke", "", () => MGLB.button_border_colour),
                new go.Binding("ButtonBorder.fill", "", () => MGLB.button_back_colour),

                $(go.TextBlock, {
                    name: "BTN_TEXT",
                    cursor: "pointer",
                    textAlign: "center",
                    margin: new go.Margin(0, 4, 0, 4),
                },
                    new go.Binding("stroke", "", () => MGLB.button_text_colour),
                    new go.Binding("font", "", () => MGLB.button_font),
                    new go.Binding("text", "", () => {
                        return ButtonType == "Note" ?
                            MGLB.button_note_text :
                            MGLB.button_detail_text
                    })
                )
            )
        );
    }
    function nt_mainContent() {
        return $(go.Panel, "Auto", {
            name: "MAIN",
            maxSize: new go.Size(parseInt(MGLB.max_node_width), parseInt(MGLB.max_node_height)),
        },
            new go.Binding("width", "node_width"),
            new go.Binding("height", "node_height"),
            $(go.Shape, "RoundedRectangle", {
                name: "SHAPE",
                fill: null,
                stroke: null,
                portId: "",
                spot1: go.Spot.TopLeft,
                spot2: go.Spot.BottomRight,
                shadowVisible: true
            },
                new go.Binding("figure", "node_corners"),
                new go.Binding("fill", "node_bg"),
                new go.Binding("stroke", "node_border_fg"),
                new go.Binding("width", "", function (data) {
                    return data.node_width - 1;
                }),
                new go.Binding("height", "", function (data) {
                    return data.node_height - 1;
                }),
            ),
            $(go.Panel, "Spot", {
                isClipping: true,
                // name: "MAIN",
            },
                $(go.Shape, {
                    name: "NODE_SHAPE",
                    portId: "",
                    cursor: "move",
                    alignment: go.Spot.Left,
                    fill: null,
                    stroke: null,
                    maxSize: new go.Size(parseInt(MGLB.max_node_width), parseInt(MGLB.max_node_height)),
                    margin: new go.Margin(0, 0, 0, 0),
                },
                    new go.Binding("figure", "node_corners"),
                    new go.Binding("fill", "node_bg"),
                    new go.Binding("stroke", "node_border_fg"),
                    new go.Binding("width", "node_width"),
                    new go.Binding("height", "node_height"),
                    new go.Binding("fill", "", function (part) {
                        return part.isHighlighted ? MGLB.node_highlight_bg : part.data.node_bg;
                    }).ofObject()
                ), //Shape

                $(go.Panel, "Table", {
                    stretch: go.GraphObject.Fill,
                    margin: new go.Margin(0, 0, 0, 0), // top, right, bottom, left
                },
                    new go.Binding("background", "", function (data, shape) {
                        return shape.part.isHighlighted ? MGLB.node_highlight_bg : shape.part.data.node_bg;
                    }).ofObject(),

                    //--- icons when button_position = left
                    $(go.Panel, "Vertical", {
                        column: 0,
                        alignment: go.Spot.TopLeft,
                        margin: new go.Margin(7, 5, 0, 0),
                    },
                    new go.Binding("alignment", "", () => {
                        switch (MGLB.image_position) {
                            case 'left':
                                return go.Spot.TopRight
                                break;
                            case 'centre':
                                return go.Spot.TopLeft;
                                break;
                            case 'right':
                                return go.Spot.TopLeft;
                                break;
                            default:
                                break;
                        }
                    }),
                        new go.Binding("visible", "", function (data) {
                            let button_position = get_btnPosition();
                            return button_position == 'left' && (data.isNote || data.show_detail);
                        }),
                        $(go.RowColumnDefinition, {
                            column: 0,
                            stretch: go.GraphObject.Horizontal,
                            alignment: go.Spot.Left,
                            // sizing: go.RowColumnDefinition.None
                        }),
                        //$(go.TextBlock, textStyle(), {
                        $(go.Picture, {
                            name: "SHOW_DETAILS_ICON_TEXT",
                            alignment: go.Spot.TopLeft,
                            margin: new go.Margin(0, 0, 0, 0), // top, right, bottom, left
                            cursor: "pointer",
                            desiredSize: new go.Size(15, 15), // Adjust the size as needed
                            mouseEnter: (e, obj) => {
                                var data = obj.part.data;
                                obj.stroke = data.node_icon_hover; // Change color to value in data
                                const hexColor = rgbToHex(data.node_icon_hover);
                                        loadSVGIcon("Icons/file-lines-regular.svg", `${hexColor}`, 250, 250).then(url => {
                                            const iconImage = obj.part.findObject("SHOW_DETAILS_ICON_TEXT");
                                            if (iconImage) {
                                                iconImage.source = url;
                                            }
                                        });
                            },
                            mouseLeave: (e, obj) => {
                                var data = obj.part.data;
                                obj.stroke = data.node_icon_fg; // Change color to value in data
                                const hexColor = rgbToHex(data.node_icon_fg);
                                        loadSVGIcon("Icons/file-lines-regular.svg", `${hexColor}`, 250, 250).then(url => {
                                            const iconImage = obj.part.findObject("SHOW_DETAILS_ICON_TEXT");
                                            if (iconImage) {
                                                iconImage.source = url;
                                            }
                                        });
                            },
                            click: (e, obj) => {
                                GetNodeInfo(obj.part);
                            },
                            toolTip: $("ToolTip",
                                $(go.TextBlock, MGLB.button_detail_text, {
                                    margin: 4
                                })
                            )
                        },
                            new go.Binding("visible", "show_detail"),
                            new go.Binding("stroke", "", function (part) {
                                return part.isHighlighted ?
                                    MGLB.node_highlight_fg : part.data.node_icon_fg;
                            }).ofObject(),
                            new go.Binding("source", "",  (obj, part) => {
                                if(obj.part.data.show_detail) {
                                    const hexColor = rgbToHex(obj.part.data.node_icon_fg);
                                    loadSVGIcon("Icons/file-lines-regular.svg", `${hexColor}`, 250, 250).then(url => {
                                        // console.log("url", url);
                                        part.source = url;
                                    });
                                }
                                return "Icons/file-lines-regular.svg";
                            }).ofObject(),
                            new go.Binding("margin", "", function (data) {
                                let button_position = get_btnPosition();
                                if (button_position == 'left')
                                    return new go.Margin(0, 1.5, 0, 4);
                                else
                                    return new go.Margin(0, 0, 0, 0);
                            })
                        ), // show detail 

                        //$(go.TextBlock, textStyle(), {
                            $(go.Picture, {
                                name: "NOTE_ICON_PICTURE",
                                alignment: go.Spot.TopLeft,
                                margin: new go.Margin(4, 0, 0, 0), // Adjust the margin as needed
                                cursor: "pointer",
                                desiredSize: new go.Size(15, 15), // Adjust the size as needed
                                // source: "Icons/clipboard-regular.svg", // Path to your SVG icon
                                mouseEnter: (e, obj, part) => {
                                    var data = obj.part.data;
                                    obj.stroke = data.node_icon_hover; // Change color to value in data
                                    const hexColor = rgbToHex(data.node_icon_hover);
                                        loadSVGIcon("Icons/paperclip-solid.svg", `${hexColor}`, 250, 250).then(url => {
                                            const iconImage = obj.part.findObject("NOTE_ICON_PICTURE");
                                            if (iconImage) {
                                                iconImage.source = url;
                                            }
                                        });
                                },
                                mouseLeave: (e, obj, part) => {
                                    var data = obj.part.data;
                                    obj.stroke = data.node_icon_fg; // Change color to value in data
                                    const hexColor = rgbToHex(data.node_icon_fg);
                                        loadSVGIcon("Icons/paperclip-solid.svg", `${hexColor}`, 250, 250).then(url => {
                                            const iconImage = obj.part.findObject("NOTE_ICON_PICTURE");
                                            if (iconImage) {
                                                iconImage.source = url;
                                            }
                                        });
                                },
                                click: (e, obj, part) => {
                                    GetNodeInfoLabel(obj.part);
                                },
                                toolTip: $("ToolTip",
                                    $(go.TextBlock, MGLB.button_note_text, {
                                        margin: 4
                                    })
                                )
                            }, 
                                new go.Binding("visible", "isNote"),
                                new go.Binding("source", "",  (obj, part) => {
                                    if(obj.part.data.isNote) {
                                        const hexColor = rgbToHex(obj.part.data.node_icon_fg);
                                        loadSVGIcon("Icons/paperclip-solid.svg", `${hexColor}`, 250, 250).then(url => {
                                            // console.log("url", url);
                                            part.source = url;
                                        });
                                    }
                                    return "Icons/paperclip-solid.svg";
                                }).ofObject(),
                                new go.Binding("margin", "", function (data) {
                                    let button_position = get_btnPosition();
                                    if (button_position == 'left')
                                        return new go.Margin(4, 1.5, 0, 4);
                                    else
                                        return new go.Margin(4, 0, 0, 0);
                                })
                            ),// Note
                            
                    ),
                    $(go.Panel, "Vertical", 
                        new go.Binding("width", "", (data) => {
                            return data.node_width - 20;
                        }),
                        new go.Binding("visible", "", (data) => {
                            if(data.node_type != 'Detail') return false; 
                            switch (MGLB.image_position) {
                                case 'left':
                                    return true;
                                    break;
                                case 'centre':
                                    return true;
                                    break;
                                case 'right':
                                    return true;
                                    break;
                                case 'inline':
                                    return false;
                                    break;
                                default:
                                    break;
                            }
                        }),
                        $(go.Panel, "Spot",
                            {
                                alignment: go.Spot.TopLeft,
                                isClipping: true,
                            },
                            new go.Binding("alignment", "", () => {
                                switch (MGLB.image_position) {
                                    case 'left':
                                        return go.Spot.Left;
                                        break;
                                    case 'centre':
                                        return go.Spot.Center;
                                        break;
                                    case 'right':
                                        return go.Spot.Right;
                                        break;
                                    default:
                                        break;
                                }
                            }),
                            $(go.Shape, {
                                fill: "white", // or any background color you prefer
                                strokeWidth: 2, // remove the border if you don't want it
                                stroke: "black",
                                },
                                new go.Binding("figure", "", function () {
                                    return MGLB.image_shape; // == 'Circle' or ''
                                }),
                                new go.Binding("stroke", "node_border_fg"),
                                new go.Binding("height", "image_height"),
                                new go.Binding("width", "image_height")
                    ),

                    //---- Picture 
                    $(go.Picture, {
                        column: 1,
                        margin: new go.Margin(0, 0, 0, 0), // top, right, bottom, left
                        background: "transparent",
                                scale: 1
                    },
                        new go.Binding("visible", "photoshow", function (t) {
                            return !!t;
                        }),
                                new go.Binding("source", "source"),
                                // new go.Binding("width", "", (data) => {
                                //     return data.picture_width;
                                // }),
                                new go.Binding("height", "image_height"),
                                new go.Binding("width", "image_height")
                            ),
                        ),
                        
                        //--------------------------------------------------------------------
    
                        //-- 6 lines of text
                        $(go.Panel, "Table", {
                            column: 2,
                            margin: new go.Margin(5, 5, 5, 5), // top, right, bottom, left
                            name: "TABLE",
                            // alignment: go.Spot.Center,
                            stretch: go.GraphObject.Horizontal,
                        },
                            $(go.RowColumnDefinition, {
                                column: 2,
                                //stretch: go.GraphObject.Horizontal,
                                // alignment: go.Spot.Left,
                            },
                        ),
                            
                            nt_createTextBlock(1),
                            nt_createTextBlock(2),
                            nt_createTextBlock(3),
                            nt_createTextBlock(4),
                            nt_createTextBlock(5),
                            nt_createTextBlock(6),
                        ),
                    ),
                    //--------------------------------------------------------------------
                    
                    //---- Picture 
                    $(go.Picture, {
                        column: 1,
                        margin: new go.Margin(0, 0, 0, 0), // top, right, bottom, left
                        background: "transparent",
                    },
                        new go.Binding("visible", "", function (data) {
                            if (data.node_type == 'Detail') {
                                switch (MGLB.image_position) {
                                    case 'left':
                                        return false && !!data.photoshow;
                                        break;
                                    case 'centre':
                                        return false && !!data.photoshow;
                                        break;
                                    case 'right':
                                        return false && !!data.photoshow;
                                        break;
                                    case 'inline':
                                        return true && !!data.photoshow;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            return !!data.photoshow;
                        }),
                        new go.Binding("source", "source"),
                    ),
                    //--------------------------------------------------------------------

                    //-- 6 lines of text
                    $(go.Panel, "Table", {
                        column: 2,
                        margin: new go.Margin(5, 5, 5, 5), // top, right, bottom, left
                        name: "TABLE",
                        //alignment: go.Spot.Center,
                        stretch: go.GraphObject.Horizontal,
                    },
                    new go.Binding("visible", "", (data) => {
                        if(data.node_type != 'Detail') return true; 
                        switch (MGLB.image_position) {
                            case 'left':
                                return false;
                                break;
                            case 'centre':
                                return false;
                                break;
                            case 'right':
                                return false;
                                break;
                            case 'inline':
                                return true;
                                break;
                            default:
                                break;
                        }
                    }),
                        $(go.RowColumnDefinition, {
                            column: 2,
                            //stretch: go.GraphObject.Horizontal,
                            //alignment: go.Spot.Left,
                        }),
                        nt_createTextBlock(1),
                        nt_createTextBlock(2),
                        nt_createTextBlock(3),
                        nt_createTextBlock(4),
                        nt_createTextBlock(5),
                        nt_createTextBlock(6),
                    ),
                    //--------------------------------------------------------------------

                ), // Table           


                $(go.Shape, {
                    name: "OUTLINE_SHAPE",
                    strokeWidth: 2.5,
                    fill: null,
                },
                    new go.Binding("figure", "node_corners"),
                    new go.Binding("stroke", "node_border_fg"),
                    new go.Binding("width", "node_width"),
                    new go.Binding("height", "node_height"),
                ), //Shape
                // ),
            ),
        );
    }
    function nt_iconContent() {
        return $(go.Panel, "Horizontal", {
            name: "HORZBTNPANEL",
            alignment: go.Spot.BottomLeft,
            alignmentFocus: go.Spot.BottomLeft,
            toolTip: new go.Adornment(),
        },
            new go.Binding("visible", "", () => {
                return MGLB.button_position !== 'none';
            }),
            new go.Binding("desiredSize", "", (data) => {
                if(MGLB.button_position == 'none') return new go.Size(0, 0);
                if(data.isCoParent && !data.isNote && !data.show_detail) return;
                if(MGLB.button_position != 'left' && ChartOrientation(BottomUp)) return new go.Size(data.node_width, 23);
            }),
            new go.Binding("alignment", "", () => {
                switch (MGLB.button_position) {
                    case 'left':
                        return go.Spot.Center;
                        break;
                    case 'bottomleft':
                        return go.Spot.BottomLeft;
                        break;
                    case 'bottomleft':
                        return go.Spot.BottomRight;
                        break;
                }
                return go.Spot.Center;
            }),
            new go.Binding("alignmentFocus", "", () => {
                switch (MGLB.button_position) {
                    case 'left':
                        return go.Spot.Center;
                        break;
                    case 'bottomleft':
                        return go.Spot.BottomLeft;
                        break;
                    case 'bottomleft':
                        return go.Spot.BottomRight;
                        break;
                }
                return go.Spot.Center;
            }),

            // can't change once done, so do all
            $(go.Panel, "Auto",
                        nt_treeExpanderButton(),
                        new go.Binding("visible", "", () => {
                    return MGLB.button_position != 'left' && ChartOrientation(TopDown, BottomUp);
                        }),
                    ),
            nt_treeButton("Note"),
            nt_treeButton("Detail"),

            //    ...(MGLB.button_position == "left" ? [
            //        nt_treeExpanderButton(),
            //    ] : [
            //        ...(MGLB.button_position == "bottomleft" ? [
            //            nt_treeExpanderButton(),
            //            nt_treeButton("Note"),
            //            nt_treeButton("Detail")
            //        ] : [
            //            nt_treeButton("Detail"),
            //            nt_treeButton("Note"),
            //            nt_treeExpanderButton()
            //        ])
            //    ]
            //    )
        );
    }
    function get_btnPosition() {
        return (MGLB.button_position !== 'none' &&
                ChartOrientation(LeftToRight, RightToLeft)) 
                ? 'left' : MGLB.button_position;
    }
    function linkOrder(node) {

        var parentNode = node.findTreeParentNode()
        if (parentNode) {
            myDiagram.startTransaction("Change zOrder");
            parentNode.findLinksOutOf().each(function (l) {
                SetProperty(l.data, "zOrder", l.toNode === node ? 10 : 1);
            });
            myDiagram.commitTransaction("Change zOrder");
        }
    }
    function highlightGroup(e, grp, show) {
        if (!grp) return;
        var selnode = myDiagram.selection.first();

        e.handled = true;
        if (show) {
            // cannot depend on the grp.diagram.selection in the case of external drag-and-drops;
            // instead depend on the DraggingTool.draggedParts or .copiedParts
            var tool = grp.diagram.toolManager.draggingTool;
            var map = tool.draggedParts || tool.copiedParts; // this is a Map
            // now we can check to see if the Group will accept membership of the dragged Parts
            if (grp.canAddMembers(map.toKeySet())) {
                grp.isHighlighted = true;
                return;
            }
        }
        grp.isHighlighted = false;
    }
    function countAllDescendants(node) {
        let count = 0; // Initialize count of descendants

        // Function to recursively count descendants
        function countDescendants(n) {
            // Use findTreeChildrenNodes to iterate over child nodes
            var children = n.findTreeChildrenNodes();
            // For each child, increment count and recurse
            children.each(function (child) {
                if (child.data.isGroup)
                    count += child.data.columns;
                else
                    count++;
                countDescendants(child);
            });
        }

        // Start counting from the initial node
        countDescendants(node);

        return count;
    }

    // calculate the nodes across in a group node even when rotated
    function makeLayout(data)
    {
        var numCols = getColumnNumber(data.columns);

        if (data.nodes_across > 0)
        {
            numCols = data.nodes_across;
            if (ChartOrientation(LeftToRight, RightToLeft))
            {
                numCols = Math.ceil(data.columns / data.nodes_across);
            }
        }

        if (numCols < 1) numCols = 3;

        return new go.GridLayout({
            wrappingColumn: numCols,
            wrappingWidth: Infinity,
            alignment: go.GridLayout.Position,
            arrangement: go.GridLayout.LeftToRight,
            sorting: go.GridLayout.Ascending,
            comparer: (pa, pb) =>
            {
                var da = pa.data;
                var db = pb.data;
                if (da.sequence < db.sequence) return -1;
                if (da.sequence > db.sequence) return 1;
                return 0;
            },
            isRealtime: false,
            cellSize: new go.Size(1, 1),
            spacing: new go.Size(10, 10),
            doLayout: function (group)
            {
                this.arrangementOrigin = group.location; // Ensure the layout starts from the group’s location

                var parts = [];
                group.memberParts.each(function (part)
                {
                    if (part instanceof go.Node)
                    { // Consider only nodes
                        parts.push(part);
                    }
                });

                // Sort the parts array by the 'sequence' property
                parts.sort(function (a, b)
                {
                    return a.data.sequence - b.data.sequence;
                });

                var numCols = getColumnNumber(group.data.columns);
                var numRows = Math.ceil(parts.length / numCols);
                var columnWidths = [];
                var rowHeights = [];

                // Find the node with the largest dimension in each row and column
                parts.forEach(function (node, index)
                {
                    var bounds = node.actualBounds;
                    var row = Math.floor(index / numCols);
                    var column = index % numCols;

                    if (rowHeights[row] == null || rowHeights[row] < bounds.height)
                    {
                        rowHeights[row] = bounds.height;
                    }
                    if (columnWidths[column] == null || columnWidths[column] < bounds.width)
                    {
                        columnWidths[column] = bounds.width;
                    }
                });

                // Apply the calculated row and column sizes
                var y = 0;
                parts.forEach(function (node, index)
                {
                    var row = Math.floor(index / numCols);
                    var column = index % numCols;
                    var x = 0;
                    for (var i = 0; i < column; i++)
                    {
                        x += columnWidths[i] + this.spacing.width;
                    }
                    if (column === 0)
                    {
                        y = row > 0 ? y + rowHeights[row - 1] + this.spacing.height : 0;
                    }
                    node.move(new go.Point(x, y));
                }, this);
            }
        })
    }


    // This method make a node as co child
    function prepareCoParentNode(parentNode, selnode) {
        var nextSeq = 0;
        if (parentNode != null) {
            nextSeq = ResequenceChildrenForGroup(parentNode);

            // This section add for setting line data
            if (selnode.data.node_type != 'Vacant' && selnode.data.node_type != 'Team') {
                prepareModelDataItemForType('CoParent', selnode, MGLB.node_height);
            }
            SetProperty(selnode.data, "group", parentNode.data.key);
            SetProperty(selnode.data, "parent", parentNode.data.key);

            SetProperty(selnode.data, "isCoParent", true);
            SetProperty(selnode.data, "isAssistant", false);
            SetProperty(selnode.data, "sequence", nextSeq);

            var nodesOut = selnode.findNodesOutOf();
            if (nodesOut.count > 0) {
                while (nodesOut.next()) {
                    nextSeq = ResequenceChildren(parentNode);
                    var nodeOut = nodesOut.value;
                    SetProperty(nodeOut.data, "parent", parentNode.data.key);
                    SetProperty(nodeOut.data, "sequence", nextSeq);
                }
            }

            var linksOut = selnode.findLinksOutOf();
            if (linksOut.count > 0) {
                while (linksOut.next()) {
                    var linkOut = linksOut.value;

                    addModelLink(parentNode.data.key, linkOut.data.to);
                    //myDiagram.model.addLinkData({ from: parentNode.data.key, to: linkOut.data.to });
                }
            }
            myDiagram.removeParts(selnode.findLinksInto());
            myDiagram.removeParts(selnode.findLinksOutOf());

        }
    }

    // This method make a node as child
    function prepareChildNode(parentNode, selnode) {
        var NextSeq = 0;
        if (parentNode != null) {
            NextSeq = ResequenceChildren(parentNode);

            SetProperty(selnode.data, "group", undefined);
            SetProperty(selnode.data, "parent", parentNode.data.key);
            SetProperty(selnode.data, "isCoParent", false);
            SetProperty(selnode.data, "isAssistant", false);
            SetProperty(selnode.data, "sequence", NextSeq);

        }
    }

    function finishDrop(e, grp) {
        var diagram = grp.diagram;
        var selnode = diagram.selection.first();
        var groupKey = grp.data.key;

        // if  node type is team, parent group or note then it will not be possible to drop
        if (selnode == null) {
            return false;
        }

        var previousGroup = myDiagram.findNodeForKey(selnode.data.group);
        if (previousGroup != null) {
            var countMem = getSubNodes(previousGroup);
            if (selnode.data.isCoParent == true && countMem.count < 2) {
                return;
            }
        }

        /* if (selnode.data.isCoParent == true) {
             return;
         }*/

        //if (selnode.data.node_type == 'ParentGroup') {
        //    e.diagram.currentTool.doCancel();
        //    return false;
        //}

        // If parent node wants to drag and drop into parent Group, it will not possible
        if (grp.data.node_type == 'ParentGroup' && grp.data.parent == selnode.data.key) {
            myDiagram.currentTool.doCancel();
            return;
        }

        if (selnode.data.node_type == 'Vacant' && selnode.data.isCoParent == true) {
            myDiagram.currentTool.doCancel();
            return;
        }
        // if (selnode.data.parent == grp.data.key && selnode.data.node_type == 'Team') {
        //     return;
        // }
        if (selnode.data.isCoParent == true && selnode.data.parent == grp.data.key) {
            return;
        }

        if (mayWorkFor(selnode, grp)) {

            // if  the parent group and selected node is in the same parent then it will drop inside the parent group
            if (((selnode.data.dragtype == 'aea' && selnode.data.parent == grp.data.key) ||
                    (selnode.data.dragtype == 'oca' && selnode.data.parent == grp.data.key) ||
                    (selnode.data.dragtype == 'oca' && selnode.data.isCoParent))) {
                var parentNode = myDiagram.findNodeForKey(groupKey);
                if (parentNode != null) {
                    prepareCoParentNode(parentNode, selnode);
                    SetProperty(selnode.data, "dragtype", 'oca');
                }
                // For changing Group layout
                SetProperty(grp.data, "columns", grp.memberParts.count);
                SetProperty(grp.data, "isTreeExpanded", true);
                resetSequenceTreeChildren(parentNode);
                resetSequenceSubGraph(parentNode);

                // Changing Previous group layout
                if (selnode.data.isCoParent == true && previousGroup != null) {
                    SetProperty(previousGroup.data, "columns", previousGroup.memberParts.count);
                }

                var ok = (grp !== null ?
                    grp.addMembers(grp.diagram.selection, true) :
                    e.diagram.commandHandler.addTopLevelParts(e.diagram.selection, true));

                if (!ok) e.diagram.currentTool.doCancel();
                ModelChanged(true);
            } else {
                // if  selected node is granchild/greatgrand child then it will drop outside of the parent group as a child
                myDiagram.startTransaction("add node as a child");
                myDiagram.removeParts(selnode.findLinksInto());
                var parentNode = myDiagram.findNodeForKey(groupKey);
                if (parentNode != null) {
                    prepareChildNode(parentNode, selnode);
                    SetProperty(parentNode.data, "isTreeExpanded", true);

                    addModelLink(parentNode.data.key, selnode.data.key);
                    //myDiagram.model.addLinkData({ from: parentNode.data.key, to: selnode.data.key });
                }

                myDiagram.commitTransaction("add node as a child");

                ModelChanged(true);
            }
        }
    }

    myDiagram.addDiagramListener("SelectionMoved", function (event) {
        if (MGLB.model_editable != false) {
            //   ModelChanged(true);
        }
    })

    function nodeStyle() {
        return [
            // The Node.location comes from the "location" property of the node data,
            // converted by the Point.parse static method.
            // If the Node.location is changed, it updates the "location" property of the node data,
            // converting back using the Point.stringify static method.
            new go.Binding("location", "location").makeTwoWay(),
            {
                // the Node.location is at the center of each node
                locationSpot: go.Spot.TopCenter
            }
        ];
    }
    myDiagram.contextMenu = myContextMenu;

    // We don't want the div acting as a context menu to have a (browser) context menu!
    cxElement.addEventListener("contextmenu", e => {
        e.preventDefault();
        return false;
    }, false);

    function hideCX() {
        if (myDiagram.currentTool instanceof go.ContextMenuTool) {
            myDiagram.currentTool.doCancel();
        }
    }

    function showContextMenu(obj, diagram, tool) {
        iawVerbose("showContextMenu");

        SuppressSelectionMove = true;

        // turn off all the context menu items and start clean, now only have to turn items on as required
        setDisplay('none',
            'add_text', 'modal_setting', 'make_group', 'assistant_on', 'assistant_off',
            'make_vacant', 'node_setting', 'move_left', 'move_right', 'make_parent_group',
            'make_new_parent', 'make_child', 'move_down_to_parent_group', 'move_up_to_parent_group', 'LabeltoolTipDIV',
            'link_settings', 'expand_below', 'collapse_below', 'delete_parent_group');

        // if there are no nodes yet, or we're not clicking on a node..

        var nodeDataArray = JSON.parse(myDiagram.model.toJson()).nodeDataArray;
        var nonLabelNodes = nodeDataArray.filter(function (node) {
            return node.node_type !== "Label";
        });
        var EmptyChart = nonLabelNodes.length == 0;

        if (EmptyChart || obj == null) {

            if (MGLB.model_editable == true) {
                if (MGLB.can_override) {
                    $ID("modal_setting").style.display = 'table-row';
                }
                $ID("add_text").style.display = 'table-row';

                if (EmptyChart)
                    $ID("make_group").style.display = 'table-row';
            }

            cxElement.classList.add("show-menu");

            // ensure menu appears within the visible screen
            positionContextMenu(diagram);

            // Optional: Use a `window` pointerdown listener with event capture to
            // remove the context menu if the user clicks elsewhere on the page
            window.addEventListener("pointerdown", hideCX, true);
            return false;
        }

        var hasParent = obj.data.parent == undefined || obj.data.parent == 0 ? false : true;


        if (obj instanceof go.Link)
        {
            if (MGLB.model_editable == true)
            {
                $ID("link_settings").style.display = 'table-row';
            }
        } else
        {

            if (obj.findTreeChildrenNodes().count > 0 && (MGLB.allow_drilldown || MGLB.model_editable)) 
            {
                if (obj.data.isTreeExpanded)
                    $ID('collapse_below').style.display = "table-row";
                else
                    $ID('expand_below').style.display = "table-row";

                // even if tree is expanded, there may be descendant nodes that
                // are collapsed, so show the expand below option anyway
                if (hasCollapsedDescendants(obj))
                    $ID('expand_below').style.display = "table-row";
            }

            if (MGLB.model_editable == true)
            {

                switch (obj.data.node_type)
                {
                    case "Detail":
                    case "Vacant":
                        if (!obj.data.isCoParent)
                        {
                            // can only do this if not in a co-parent node
                            if (obj.data.isAssistant)
                                $ID("assistant_on").style.display = hasParent ? 'table-row' : 'none';
                            else
                                $ID("assistant_off").style.display = hasParent ? 'table-row' : 'none';

                            $ID("make_group").style.display = 'table-row';
                            $ID("make_parent_group").style.display = 'table-row';

                            if (obj.findTreeChildrenNodes().count == 1)
                            {
                                var childNode = obj.findTreeChildrenNodes().first();
                                if (childNode.data.node_type == 'ParentGroup')
                                {
                                    $ID("move_down_to_parent_group").style.display = 'table-row';
                                }
                            }
                            if (obj.findTreeParentNode() && obj.findTreeParentNode().data.node_type == 'ParentGroup') {
                                $ID("move_up_to_parent_group").style.display = 'table-row';
                            }

                        }
                        if (obj.data.isCoParent)
                        {
                            var groupNode = myDiagram.findNodeForKey(obj.data.parent);
                            if (groupNode.memberParts.count > 1)
                            {
                                $ID("make_new_parent").style.display = 'table-row';
                                $ID("make_child").style.display = 'table-row';

                            }
                        }
                        $ID("node_setting").style.display = 'table-row';

                        if (obj.data.node_type == 'Detail')
                        {
                            $ID("make_vacant").style.display = 'table-row';
                            //$ID("node_detail").style.display = obj.data.show_detail ? 'block' : 'none';
                        }
                        break;

                    case "Team":
                        $ID("node_setting").style.display = 'table-row';
                        if (!obj.data.isCoParent)
                        {
                            // can only do this if not in a co-parent node
                            if (obj.data.isAssistant)
                                $ID("assistant_on").style.display = hasParent ? 'table-row' : 'none';
                            else
                                $ID("assistant_off").style.display = hasParent ? 'table-row' : 'none';

                            $ID("make_group").style.display = 'table-row';
                            $ID("make_parent_group").style.display = 'table-row';

                            if (obj.findTreeChildrenNodes().count == 1)
                            {
                                var childNode = obj.findTreeChildrenNodes().first();
                                if (childNode.data.node_type == 'ParentGroup')
                                {
                                    $ID("move_down_to_parent_group").style.display = 'table-row';
                                }
                                }
                            if (obj.findTreeParentNode() && obj.findTreeParentNode().data.node_type == 'ParentGroup') {
                                $ID("move_up_to_parent_group").style.display = 'table-row';
                            }

                        }
                        if (obj.data.isCoParent)
                        {
                            var groupNode = myDiagram.findNodeForKey(obj.data.parent);
                            if (groupNode.memberParts.count > 1)
                            {
                                $ID("make_new_parent").style.display = 'table-row';
                                $ID("make_child").style.display = 'table-row';

                            }
                        }
                        break;
                    case "ParentGroup":
                        if (obj.data.isAssistant)
                            $ID("assistant_on").style.display = hasParent ? 'table-row' : 'none';
                        else
                            $ID("assistant_off").style.display = hasParent ? 'table-row' : 'none';

                        $ID("node_setting").style.display = 'table-row';
                        $ID("make_group").style.display = 'table-row';
                        let subNodes = [];
                        for (var mit = obj.memberParts; mit.next();) {
                            var part = mit.value; // part is now a Part within the Group
                            if (part instanceof go.Node) {
                                subNodes.push(part);
                            }
                        }
                        if (subNodes.length == 1) {
                            $ID("delete_parent_group").style.display = 'table-row';
                        }
                        break;

                    case "Label":
                        $ID("node_setting").style.display = 'table-row';
                        //$ID("label_detail").style.display = 'block';
                        break;
                }

                // now deal with the move left / move right

                switch (obj.data.node_type)
                {
                    case "Vacant":
                    case "Detail":
                    case "Team":
                    case "ParentGroup":
                        var LR = checkNodeLR(obj);
                        if (LR.canLeft)
                            $ID("move_left").style.display = 'table-row';
                        if (LR.canRight)
                            $ID("move_right").style.display = 'table-row';
                        break;
                }
            }
        }

        var hasMenuItem = false;

        function maybeShowItem(elt, pred) {
            if (pred) {
                hasMenuItem = true;
            } else {
                elt.style.display = "none";
            }
        }

        maybeShowItem($ID("color"), obj !== null);

        // Now show the whole context menu element
        if (hasMenuItem) {
            cxElement.classList.add("show-menu");

            // ensure menu appears within the visible screen
            positionContextMenu(diagram);
        }

        // Optional: Use a `window` pointerdown listener with event capture to
        //           remove the context menu if the user clicks elsewhere on the page
        window.addEventListener("pointerdown", hideCX, true);
    }

    function hideContextMenu() {
        iawVerbose("hideContextMenu");
        SuppressSelectionMove = false;

        cxElement.classList.remove("show-menu");
        // Optional: Use a `window` pointerdown listener with event capture to
        //           remove the context menu if the user clicks elsewhere on the page
        window.removeEventListener("pointerdown", hideCX, true);
    }

    function positionContextMenu(diagram) {
        // determine whether we are to the left or right of the middle of the diagram and
        // whether we are above or below the mid point of the diagram.
        // if left then display meny to the left of the click point, else display to the right
        // if above then display above click point, else display below
        var mousePt = diagram.lastInput.viewPoint;
        var VRect = myDiagram.viewportBounds;
        var centerX = (VRect.centerX - VRect.left) * myDiagram.scale;
        var centerY = (VRect.centerY - VRect.top) * myDiagram.scale;

        if (mousePt.y < centerY) {
            cxElement.style.top = mousePt.y + "px";
        } else {
            cxElement.style.top = (mousePt.y - cxElement.offsetHeight) + "px";
        }
        if (mousePt.x < centerX) {
            cxElement.style.left = mousePt.x + "px";
        } else {
            cxElement.style.left = (mousePt.x - cxElement.offsetWidth) + "px";
        }
    }

    function showLabelToolTip(obj, diagram, tool) {
        const collection = $ID('LabeltoolTipDivPra').children;
        var n = collection.length;

        var toolText, toolFg, toolBg, toolBorder;
        if (obj instanceof go.Link) {
            toolText = obj.toNode.data.linkTooltip;
            toolFg = obj.toNode.data.linkTooltipForeground === "" ? MGLB.node_tt_fg : obj.toNode.data.linkTooltipForeground;
            toolBg = obj.toNode.data.linkTooltipBackground === "" ? MGLB.node_tt_bg : obj.toNode.data.linkTooltipBackground;
            toolBorder = "1px solid " + (obj.toNode.data.linkTooltipBorder === "" ? MGLB.node_tt_border : obj.toNode.data.linkTooltipBorder);
        } else {
            toolText = obj.data.tooltip;
            toolFg = obj.data.node_tt_fg === "" ? MGLB.node_tt_fg : obj.data.node_tt_fg;
            toolBg = obj.data.node_tt_bg === "" ? MGLB.node_tt_bg : obj.data.node_tt_bg;
            toolBorder = "1px solid " + (obj.data.node_tt_border === "" ? MGLB.node_tt_border : obj.data.node_tt_border);
        }

        if (toolText === undefined || toolText === "") return;

        var LabletoolTipDIV = $ID('LabeltoolTipDIV');
        $ID('LabeltoolTipDivPra').innerHTML = toolText;

        // Initially display tooltip to calculate width
        LabletoolTipDIV.style.display = "block";
        var tooltipWidth = LabletoolTipDIV.offsetWidth;

        // Use diagram's viewportBounds.width to ensure tooltip fits within the diagram's viewport
        var viewportWidth = diagram.viewportBounds.width; // Adjusted to use diagram's viewport width
        var pt = diagram.lastInput.viewPoint;

        // Calculate desired left position to prevent tooltip from overflowing the right edge of the viewport
        var desiredLeft = pt.x + 10 + tooltipWidth > viewportWidth ? Math.max(viewportWidth - tooltipWidth - 20, 0) : pt.x + 10;

        LabletoolTipDIV.style.left = desiredLeft + "px";
        LabletoolTipDIV.style.top = (pt.y + 10) + "px";
        LabletoolTipDIV.style.padding = "5px";
        LabletoolTipDIV.style.color = toolFg;
        LabletoolTipDIV.style.backgroundColor = toolBg;
        LabletoolTipDIV.style.border = toolBorder;

        for (var i = 0; i < n; i++) {
            if (collection[i] !== undefined) {
                collection[i].style.color = toolFg;
            }
        }
    }

    function LabelhideToolTip(diagram, tool) {
        var LabletoolTipDIV = $ID('LabeltoolTipDIV');
        LabletoolTipDIV.style.display = "none";
    }

    function NodeLineVisibility(nodeData, line) {
        // Initialize min_line and max_line to cover the possible range of lines
        var min_line = 7; // Set to a value higher than the maximum possible line
        var max_line = 0; // Set to 0, will be updated with the highest non-empty line

        // if detail node then range default to data_view_display range
        if (nodeData.node_type.toLowerCase() == 'detail') {
            min_line = 1;
            max_line = MGLB.display_lines;
        }

        // Check each line from 1 to 6 to find the minimum and maximum non-empty lines
        for (var i = 1; i <= 6; i++) {
            if (nodeData['line' + i] != '') {
                if (i > max_line) max_line = i;
                if (min_line === 7) min_line = i;
            }
        }

        // Check if the line is within the range of non-empty lines
        return (line >= min_line && line <= max_line);
    }

    // define the Link template
    myDiagram.linkTemplate =
        $(go.Link, go.Link.Orthogonal, {
            corner: 5,
            zOrder: 1,
            selectable: true,
            relinkableFrom: false,
            relinkableTo: false,
            toolTip: myLabelToolTip,
            contextMenu: myContextMenu,
            selectionAdornmentTemplate: $(go.Adornment),
            mouseEnter: function (e, link) {
                link.path.stroke = link.toNode.data.linkHover;
                link.path.strokeWidth = 8;
            },
            mouseLeave: function (e, link) {
                link.path.stroke = link.toNode.data.linkColour;
                link.path.strokeWidth = link.toNode.data.linkWidth;
            },
            click: function (e, link) {
                linkOrder(link.toNode);
            },
        },
            new go.Binding("zOrder"),
            $(go.Shape, {
                name: "OBJSHAPE",
            },
                new go.Binding("stroke", "", function (data, link) {
                    if (link.part.toNode.data.node_type == 'Label') return;
                    return link.part.toNode.data.linkColour;
                }),
                new go.Binding("strokeDashArray", "", function (data, link) {
                    if (link.part.toNode.data.node_type == 'Label') return;
                    return GetLinkLineType(link.part.toNode.data);
                }),
                new go.Binding("strokeWidth", "", function (data, link) {
                    if (link.part.toNode.data.node_type == 'Label') return;
                    return link.part.toNode.data.linkWidth;
                })
            ),
        );

    myDiagram.alignDocument(go.Spot.Top, go.Spot.Top);

    if (MGLB.model_editable != true) {
        myDiagram.initialAutoScale = go.Diagram.Uniform;
        myDiagram.commandHandler.zoomToFit();
        myDiagram.requestUpdate();
    } else {
        myDiagram.scale = 1;
    }

    // But the Diagrams share the same undo manager!

    myDiagram.model.undoManager.isEnabled = false;

    zoomSlider = new ZoomSlider(myDiagram, {
        size: 100,
        buttonSize: 20,
        opacity: 0.5
    });

    zoomSlider.zoomSliderOut.style.backgroundColor = "#000";
    zoomSlider.zoomSliderIn.style.backgroundColor = "#000";

    myDiagram.commandHandler.doKeyDown = function () {
        if (!myDiagram.isModelReadOnly) {
            var e = myDiagram.lastInput;

            var keycode = e.event.keyCode;
            var controlkey = (e.control || e.meta);

            var obj = myDiagram.selection.first();
            if (obj) {
                if (e.control || e.meta && (keycode == 37 || keycode == 39)) {
                    var LR = checkNodeLR(obj);
                    var UDLR = checkNodeUDLR(obj);
                    if (keycode == 37) { // Ctrl left arrow
                        if (LR.canLeft) SwitchNode(obj, "Left");
                        return;
                    }
                    if (keycode == 39) { // Ctrl right arrow
                        if (LR.canRight) SwitchNode(obj, "Right");
                        return;
                    }
                    if (keycode == 38) { // up arrow
                        if (UDLR.canUp) MoveUpToParentGroup(obj);
                        return;
                    }
                    if (keycode == 40) { // down arrow
                        if (UDLR.canDown) MoveToParentGroup(obj);
                        return;
                    }
                }

                if (!e.shift && !controlkey) {
                    if (keycode == 37 || keycode == 38 || keycode == 39 || keycode == 40) {
                        var UDLR = checkNodeUDLR(obj);
                        if (keycode == 37) { // left arrow
                            if (UDLR.canLeft) myDiagram.select(myDiagram.findNodeForKey(UDLR.prevKey));
                            return;
                        }
                        if (keycode == 39) { // right arrow
                            if (UDLR.canRight) myDiagram.select(myDiagram.findNodeForKey(UDLR.nextKey));
                            return;
                        }
                        if (keycode == 38) { // up arrow
                            if (UDLR.canUp) myDiagram.select(myDiagram.findNodeForKey(UDLR.upKey));
                            return;
                        }
                        if (keycode == 40) { // down arrow
                            if (UDLR.canDown) myDiagram.select(myDiagram.findNodeForKey(UDLR.downKey));
                            return;
                        }
                    }
                }
            }

            // call base method with no arguments (default functionality)
            go.CommandHandler.prototype.doKeyDown.call(this);
        }
    };

} // end init
function addModelLink(linkFrom, linkTo) {
    myDiagram.model.addLinkData({
        from: linkFrom,
        to: linkTo,
        // colour: newColor // Set the color here
    });
}
function isAssistant(n) {
    if (n === null) return false;
    return n.data.isAssistant;
}
function ResizeDiv(el) {
    var divo = $ID(el);

    if (divo) {
        var vh = (window.innerHeight || document.documentElement.clientHeight),
            rect = divo.getBoundingClientRect(),
            sTop = window.scrollY || document.documentElement.scrollTop;
        divo.style.height = (vh - rect.top + sTop) + "px";
    }
}
function GetSortOrder(prop) {
    return function (a, b) {
        if (a[prop] > b[prop]) {
            return 1;
        } else if (a[prop] < b[prop]) {
            return -1;
        }
        return 0;
    }
}
function checkNodeUDLR(obj) {
    // look whether you can use shift-arrow to move up/down/left/right in the model

    var PrevKey = -99999,
        NextKey = -99999,
        UpKey = -99999,
        DownKey = -99999;
    var canLeft = false,
        canRight = false,
        canUp = false,
        canDown = false;
    var noNodes = [],
        asNodes = [],
        actNodes = [];
    var chld;
    var FoundNode = 0;
    var ActualNodesCount = 0;

    if (obj.data.node_type == 'Label')
        return;


    if (obj.data.isCoParent) {
        var ParentNode = myDiagram.findNodeForKey(obj.data.parent);
        if (ParentNode == null || getSubNodes(ParentNode).count < 1)
            return {
                'canLeft': canLeft,
                'canRight': canRight
            };

        chld = getSubNodes(ParentNode).iterator;
        while (chld.next()) {
            actNodes.push(chld.value);
        }
        actNodes.sort((a, b) => {
            return a.data.sequence - b.data.sequence;
        });

        actNodes.forEach((e) => {
            switch (FoundNode) {
                case 0:
                    ActualNodesCount += 1;
                    if (e.data.key == obj.data.key)
                        FoundNode = 1;
                    else
                        PrevKey = e.data.key;
                    break;
                case 1:
                    ActualNodesCount += 1;
                    NextKey = e.data.key;
                    FoundNode = 2;
                    break;
            }
        });

    } else {
        var ParentNode = obj.findTreeParentNode();
        if (ParentNode) {
            canUp = true;
            UpKey = ParentNode.data.key;

            chld = ParentNode.findTreeChildrenNodes();
            while (chld.next()) {
                if (chld.value.data.isAssistant)
                    asNodes.push(chld.value);
                else
                    noNodes.push(chld.value);
            }
            asNodes.sort((a, b) => {
                return a.data.sequence - b.data.sequence;
            });
            noNodes.sort((a, b) => {
                return a.data.sequence - b.data.sequence;
            });

            // now add the sorted nodes, assistant ones first then the normal ones
            actNodes = [];
            actNodes.push.apply(actNodes, asNodes);
            actNodes.push.apply(actNodes, noNodes);

            // Look through the child nodes looking for the current node.
            // if the node we're looking at isn't the correct one, store it as the move left node
            // when we do find our node, get the next child and set it as the move right node.
            // FoundNode starts at 0 so it will keep looking for the current node
            //           changes to 1 when we find the current node and next loop will store the node as the Next
            //           and finally sets the FoundNode to 2 just to go around the rest of the loop without doing anything.

            actNodes.forEach((e) => {
                switch (FoundNode) {
                    case 0:
                        ActualNodesCount += 1;
                        if (e.data.key == obj.data.key)
                            FoundNode = 1;
                        else
                            PrevKey = e.data.key;
                        break;
                    case 1:
                        ActualNodesCount += 1;
                        NextKey = e.data.key;
                        FoundNode = 2; // just loop round after this point
                        break;
                }
            });
        }

        // now we need to see if we can arrow down
        asNodes = [];
        noNodes = [];
        actNodes = [];

        chld = obj.findTreeChildrenNodes();
        while (chld.next()) {
            if (chld.value.data.isAssistant)
                asNodes.push(chld.value);
            else
                noNodes.push(chld.value);
        }
    }

    asNodes.sort((a, b) => {
        return a.data.sequence - b.data.sequence;
    });
    noNodes.sort((a, b) => {
        return a.data.sequence - b.data.sequence;
    });

    // now add the sorted nodes, assistant ones first then the normal ones
    actNodes.push.apply(actNodes, asNodes);
    actNodes.push.apply(actNodes, noNodes);

    // we just want to grab the very first node, if there is one

    actNodes.forEach((e) => {
        if (DownKey == -99999) {
            DownKey = e.data.key;
            canDown = true;
        }
    });


    // if a previous sibling node exists then show the move left entry if more than one node
    canLeft = PrevKey != -99999 && ActualNodesCount > 1;

    // if a next sibling node exists then show the move right entry if more than one node
    canRight = NextKey != -99999 && ActualNodesCount > 1;

    return {
        'canLeft': canLeft,
        'prevKey': PrevKey,
        'canRight': canRight,
        'nextKey': NextKey,
        'canUp': canUp,
        'upKey': UpKey,
        'canDown': canDown,
        'downKey': DownKey
    };
}
function checkNodeLR(obj) {
    var PrevKey = -99999,
        NextKey = -99999;
    var canLeft = false,
        canRight = false;
    var actNodes = [];
    var chld;
    var FoundNode = 0;
    var ActualNodesCount = 0;

    if (obj.data.node_type == 'Label')
        return;


    if (obj.data.isCoParent) {
        var ParentNode = myDiagram.findNodeForKey(obj.data.parent);

        if (ParentNode == null || getSubNodes(ParentNode).count < 2)
            return {
                'canLeft': canLeft,
                'canRight': canRight
            };

        chld = getSubNodes(ParentNode).iterator;
        while (chld.next()) {
            actNodes.push(chld.value);
        }
        actNodes.sort((a, b) => {
            return a.data.sequence - b.data.sequence;
        });

        actNodes.forEach((e) => {
            switch (FoundNode) {
                case 0:
                    ActualNodesCount += 1;
                    if (e.data.key == obj.data.key)
                        FoundNode = 1;
                    else
                        PrevKey = e.data.key;
                    break;
                case 1:
                    ActualNodesCount += 1;
                    NextKey = e.data.key;
                    FoundNode = 2;
                    break;
            }
        });

    } else {
        var ParentNode = obj.findTreeParentNode();
        var orgIsAssistant = obj.data.isAssistant;

        if (ParentNode == null || ParentNode.findTreeChildrenNodes().count < 2)
            return {
                'canLeft': canLeft,
                'canRight': canRight
            };

        chld = ParentNode.findTreeChildrenNodes();
        while (chld.next()) {
            if (orgIsAssistant != chld.value.data.isAssistant) continue;
            actNodes.push(chld.value);
        }
        actNodes.sort((a, b) => {
            return a.data.sequence - b.data.sequence;
        });

        // Look through the child nodes looking for the current node.
        // if the node we're looking at isn't the correct one, store it as the move left node
        // when we do find our node, get the next child and set it as the move right node.
        // FoundNode starts at 0 so it will keep looking for the current node
        //           changes to 1 when we find the current node and next loop will store the node as the Next
        //           and finally sets the FoundNode to 2 just to go around the rest of the loop without doing anything.

        actNodes.forEach((e) => {
            switch (FoundNode) {
                case 0:
                    ActualNodesCount += 1;
                    if (e.data.key == obj.data.key)
                        FoundNode = 1;
                    else
                        PrevKey = e.data.key;
                    break;
                case 1:
                    ActualNodesCount += 1;
                    NextKey = e.data.key;
                    FoundNode = 2; // just loop round after this point
                    break;
            }
        });

    }



    // if a previous sibling node exists then show the move left entry if more than one node
    canLeft = PrevKey != -99999 && ActualNodesCount > 1;

    // if a next sibling node exists then show the move right entry if more than one node
    canRight = NextKey != -99999 && ActualNodesCount > 1;

    return {
        'canLeft': canLeft,
        'prevKey': PrevKey,
        'canRight': canRight,
        'nextKey': NextKey
    };
}
function ResequenceChildren(ParentNode) {
    iawVerbose("ResequenceChildren");
    var chld;
    var ChildSeq = 0;
    if (ParentNode) {
        chld = ParentNode.findTreeChildrenNodes();
        while (chld.next()) {
            if (chld.value.data.isAssistant == false) {
                ChildSeq += 1;
                chld.value.data.sequence = ChildSeq;
            }
        }
    }
    return ChildSeq + 1;
}
function ResequenceAssistants(ParentNode) {
    iawVerbose("ResequenceAssistants");
    var chld;
    var ChildSeq = 0;
    if (ParentNode) {
        chld = ParentNode.findTreeChildrenNodes();
        while (chld.next()) {
            if (chld.value.data.isAssistant == true) {
                ChildSeq += 1;
                chld.value.data.sequence = ChildSeq;
            }
        }
    }
    return ChildSeq + 1;
}
function ResequenceTheLastChildren(ParentNode) {
    var chld;
    var ChildSeq = 0;
    if (ParentNode) {
        chld = ParentNode.findTreeChildrenNodes();
        while (chld.next()) {
            ChildSeq += 1;
        }
    }
    return ChildSeq + 1;
}
function resetSequenceTreeChildren(ParentNode) {
    var childs;
    var ChildSeq = 1;
    if (ParentNode) {
        childs = ParentNode.findTreeChildrenNodes();
        while (childs.next()) {
            SetProperty(childs.value.data, "sequence", ChildSeq);
            ChildSeq += 1;
        }
    }
}
function resetSequenceSubGraph(ParentNode) {
    var childs;
    var ChildSeq = 1;
    if (ParentNode) {
        var childs = getSubNodes(ParentNode).iterator;
        while (childs.next()) {
            SetProperty(childs.value.data, "sequence", ChildSeq);
            ChildSeq += 1;
        }
    }
}
function ResequenceChildrenForGroup(ParentNode) {
    var chld;
    var ChildSeq = 0;
    if (ParentNode) {
        if (ParentNode.data.isGroup == true && ParentNode.data.node_type == "ParentGroup") {
            var memberList = getSubNodes(ParentNode).iterator;
            while (memberList.next()) {
                ChildSeq += 1;
                memberList.value.data.sequence = ChildSeq;
            }
        }
    }
    return ChildSeq + 1;
}
function getSubNodes(g) {
    var memberList = new go.List();
    g.memberParts.filter(function (p) {
        if (p instanceof go.Node) {
            memberList.push(p);
        }
    });

    // Sort the list based on the node's data.sequence property
    memberList.sort(function (a, b)
    {
        return a.data.sequence - b.data.sequence;
    });

    return memberList;
}
function BuildAEA() {
    var i;

    //$ID("txtAEAfilter").value = "";
    $ID("ulUnallocated").innerHTML = "";

    // sort the AEA according
    AEAList.sort(function (a, b) {
        var left = a.sort_name.split("|")[AEASortColumn];
        var right = b.sort_name.split("|")[AEASortColumn];

        if (AEASortAscending)
            return left.localeCompare(right);
        else
            return right.localeCompare(left);
    });

    //AEAList.map((item, key) => {
    //    $ID("ulUnallocated").innerHTML +=
    //        '<tr id="item' + item.item_ref + '" ondragover="allowDrop(event)" draggable="true" class="listrow aea_li">' +
    //        '</tr > '

    //    var cols = item.name.split("|");
    //    var align = MGLB.AEAColsAlign.split("|"); // AEAColsAlign contains left|right|center etc

    //    for (i = 0; i < cols.length; i++) {
    //        $ID("item" + item.item_ref).innerHTML += '<td class="' + align[i] + ' nowrap">' + cols[i] + '</td>';
    //    }
    //});

    // Assuming AEAList and MGLB are defined elsewhere
    const fragment = document.createDocumentFragment();

    AEAList.forEach((item) =>
    {
        // Create a new row for each item
        const row = document.createElement('tr');
        row.id = 'item' + item.item_ref;
        row.className = 'listrow aea_li';
        row.setAttribute('ondragover', 'allowDrop(event)');
        row.setAttribute('draggable', 'true');

        // Split the item's name and alignment settings
        const cols = item.name.split("|");
        const align = MGLB.AEAColsAlign.split("|");

        // Create and append cells to the row
        cols.forEach((col, i) =>
        {
            const cell = document.createElement('td');
            cell.className = align[i] + ' nowrap';
            cell.textContent = col;
            row.appendChild(cell);
        });

        // Append the row to the DocumentFragment
        fragment.appendChild(row);
    });

    // Append the entire fragment to the table body in one operation
    document.getElementById('ulUnallocated').appendChild(fragment);



    if (MGLB.model_editable == true && AEAList.length > 0)
        PanelShowLeft(false);
    else
        PanelHideLeft(false);
    // re-apply any filter to the list 
    FilterAEA();
}
function FilterAEA() {
    var searchText = $('#txtAEAfilter').val().toLocaleLowerCase();
    var normalRow = true;

    $('#ulUnallocated > tr').each(function () {
        var showCurrentLi = true;
        if (searchText != "") {
            var currentLiText = $(this).text().toLocaleLowerCase();
            showCurrentLi = currentLiText.indexOf(searchText) !== -1;
        }
        $(this).removeClass("hide");
        if (!showCurrentLi) {
            $(this).addClass("hide");
        } else {
            $(this).removeClass("listrow listaltrow");
            $(this).addClass(normalRow ? "listrow" : "listaltrow");
            normalRow = !normalRow;
        }
    });
}
function filterGridViewRows() {
    if (!document.getElementById('txtSearch')) return;
    var searchValue = document.getElementById('txtSearch').value.toLowerCase();
    var rows = document.querySelectorAll('#grdEmps tr');
    var normalRow = true;

    for (var i = 0; i < rows.length; i++) {

        var currentRow = rows[i];
        var text = currentRow.textContent || currentRow.innerText;

        if (text.toLowerCase().indexOf(searchValue) > -1) {
            currentRow.style.display = ''; // Show the row
            currentRow.className = normalRow ? 'listrow' : 'listaltrow';
            normalRow = !normalRow;

        } else {
            currentRow.style.display = 'none'; // Hide the row
        }
    }
    setSessionValue("MasterPageSearch", searchValue);
}
function GetDetailRootNode() {
    var rootNode;
    myDiagram.nodes.each(node => {
        var linksInto = node.findLinksInto();
        if (node.data.node_type !== "Label" && node.data.isCoParent == false) {
            if (linksInto.count === 0 ||
                (linksInto.count === 1 && linksInto.first() && !linksInto.first().fromNode)) {
                rootNode = node;
            }
        }
    });
    return rootNode;
}
function CalcNodeTableWidth(node_type, show_photo, node_width, picture_width) {
    // calculate the width of the text part of a node (will be wider if no photo needed)
    if (MGLB.show_photos == false ||
        MGLB.photos_applicable == false ||
        show_photo == false ||
        node_type == 'Team' ||
        node_type == 'Vacant')
        return parseInt(node_width);
    else
        return parseInt(node_width - picture_width);
}
function getColumnNumber(numNodes) {

    var MAX_ACROSS_TOP_DOWN = [1, 2, 3, 2, 3, 3, 4, 4, 3];

    return ChartOrientation(TopDown, BottomUp) ?
        (numNodes < 10 ? MAX_ACROSS_TOP_DOWN[numNodes - 1] : 4) :
        (numNodes < 6 ? 1 : 2);
}

//----------------------------------------------------------------------------------------------------------------------
// Dragging Events
//----------------------------------------------------------------------------------------------------------------------
var dragged = null;

function addDragEvents() {
    document.addEventListener("dragstart", event => {

        event.dataTransfer.setData("item_ref", event.target.id.substr(4));

        // store a reference to the dragged element and the offset of the mouse from the center of the element

        dragged = event.target;
        dragged.offsetX = event.offsetX - dragged.clientWidth / 2;
        dragged.offsetY = event.offsetY - dragged.clientHeight / 2;

        event.target.style.border = "2px solid red";

    }, false);

    document.addEventListener("dragend", event => {
        // reset the border of the dragged element
        if (dragged != null)
            dragged.style.border = "";

        onHighlight(null, event);
    }, false);

    $ID("myDiagramDiv").addEventListener("dragenter", event => {
        // Here you could also set effects on the Diagram,
        // such as changing the background color to indicate an acceptable drop zone
        // Requirement in some browsers, such as Internet Explorer
        event.preventDefault();
    }, false);
    $ID("myDiagramDiv").addEventListener("dragover", event => {
        // We call preventDefault to allow a drop
        // But on divs that already contain an element,
        // we want to disallow dropping

        if ($ID("myDiagramDiv") === myDiagram.div) {
            var can = event.target;
            var pixelratio = myDiagram.computePixelRatio();
            // if the target is not the canvas, we may have trouble, so just quit:
            if (!(can instanceof HTMLCanvasElement)) return;
            var bbox = can.getBoundingClientRect();
            var bbw = bbox.width;
            if (bbw === 0) bbw = 0.001;
            var bbh = bbox.height;
            if (bbh === 0) bbh = 0.001;
            var mx = event.clientX - bbox.left * ((can.width / pixelratio) / bbw);
            var my = event.clientY - bbox.top * ((can.height / pixelratio) / bbh);
            var point = myDiagram.transformViewToDoc(new go.Point(mx, my));
            var part = myDiagram.findPartAt(point, true);
            if (part != null &&
                (part.data.isCoParent == false ||
                    (part.data.isCoParent == true && part.data.node_type == 'Vacant')
                )
            ) {
                onHighlight(part, event);
            }

        }

        if (event.target.className === "dropzone") {
            // Disallow a drop by returning before a call to preventDefault:
            return;
        }
        // Allow a drop on everything else
        event.preventDefault();
    }, false);
    $ID("myDiagramDiv").addEventListener("dragleave", event => {
        // reset background of potential drop target

        if (event.target.className == "dropzone") {
            event.target.style.background = "";
        }

        onHighlight(null, event);
    }, false);

    // handle the user option for removing dragged items from the Palette
    $ID("myDiagramDiv").addEventListener("drop", event => {
        iawVerbose("myDiagramDiv.eventlistener.drop - AEA onto OCA");

        event.preventDefault();

        SuppressSelectionMove = true;

        if ($ID("myDiagramDiv") === myDiagram.div) {
            // Drag AEA onto OCA

            var can = event.target;
            var pixelratio = myDiagram.computePixelRatio();

            // if the target is not the canvas, we may have trouble, so just quit:
            if (!(can instanceof HTMLCanvasElement)) return;

            var bbox = can.getBoundingClientRect();
            var bbw = bbox.width;
            if (bbw === 0) bbw = 0.001;

            var bbh = bbox.height;
            if (bbh === 0) bbh = 0.001;

            var mx = event.clientX - bbox.left * ((can.width / pixelratio) / bbw);
            var my = event.clientY - bbox.top * ((can.height / pixelratio) / bbh);
            var point = myDiagram.transformViewToDoc(new go.Point(mx, my));

            var OCAData = GetModelDataItem(event.dataTransfer.getData('item_ref'), MGLB.node_height);

            if (dragged != null) {
                // split the line_attr down to the individual 6 lines for font and underline
                var attr = GetLineAttrs(MGLB.line_attr);
                var newdata = {
                    location: myDiagram.transformViewToDoc(new go.Point(mx - dragged.offsetX, my - dragged.offsetY)),
                    item_ref: event.dataTransfer.getData('item_ref'),

                    line_attr: MGLB.line_attr,

                    line1: OCAData.line1,
                    line2: OCAData.line2,
                    line3: OCAData.line3,
                    line4: OCAData.line4,
                    line5: OCAData.line5,
                    line6: OCAData.line6,

                    ds_line1: OCAData.line1,
                    ds_line2: OCAData.line2,
                    ds_line3: OCAData.line3,
                    ds_line4: OCAData.line4,
                    ds_line5: OCAData.line5,
                    ds_line6: OCAData.line6,

                    font1: attr.font[1],
                    font2: attr.font[2],
                    font3: attr.font[3],
                    font4: attr.font[4],
                    font5: attr.font[5],
                    font6: attr.font[6],

                    isUnderline1: attr.isUnderline[1],
                    isUnderline2: attr.isUnderline[2],
                    isUnderline3: attr.isUnderline[3],
                    isUnderline4: attr.isUnderline[4],
                    isUnderline5: attr.isUnderline[5],
                    isUnderline6: attr.isUnderline[6],

                    fontColour1: attr.colour[1],
                    fontColour2: attr.colour[2],
                    fontColour3: attr.colour[3],
                    fontColour4: attr.colour[4],
                    fontColour5: attr.colour[5],
                    fontColour6: attr.colour[6],

                    fontBgColour1: attr.bg_colour[1],
                    fontBgColour2: attr.bg_colour[2],
                    fontBgColour3: attr.bg_colour[3],
                    fontBgColour4: attr.bg_colour[4],
                    fontBgColour5: attr.bg_colour[5],
                    fontBgColour6: attr.bg_colour[6],

                    alignment1: attr.align[1],
                    alignment2: attr.align[2],
                    alignment3: attr.align[3],
                    alignment4: attr.align[4],
                    alignment5: attr.align[5],
                    alignment6: attr.align[6],

                    source: OCAData.node_picture,
                    show_detail: OCAData.show_detail,
                    node_bg: MGLB.node_bg,
                    node_fg: MGLB.node_fg,
                    node_border_fg: MGLB.node_border_fg,
                    node_text_bg: MGLB.node_text_bg,
                    node_text_bg_block: MGLB.node_text_bg_block,
                    node_icon_fg: MGLB.node_icon_fg,
                    node_icon_hover: MGLB.node_icon_hover,
                    node_height: parseInt(MGLB.node_height),
                    node_width: parseInt(MGLB.node_width),
                    node_corners: MGLB.node_corners,
                    show_photos: MGLB.show_photos,
                    image_height: MGLB.image_height,
                    photoshow: MGLB.show_photos,
                    visible: MGLB.show_photos,
                    tooltip: "",
                    existnode: "0",
                    dragtype: "aea",
                    node_tt_bg: MGLB.node_tt_bg,
                    node_tt_border: MGLB.node_tt_border,
                    node_tt_fg: MGLB.node_tt_fg,
                    margin: new go.Margin(0, 5, 0, 5),
                    isTreeExpanded: true,
                    node_type: "Detail",
                    isNote: false,
                    sequence: 0,
                    isAssistant: false,
                    isCoParent: false,
                    picture_width: parseInt(OCAData.picture_width),
                    node_table_width: CalcNodeTableWidth("Detail",
                        MGLB.show_photos,
                        parseInt(MGLB.node_width),
                        parseInt(OCAData.picture_width)),
                    showShadow: MGLB.showShadow,
                    shadowColour: MGLB.shadowColour,
                    linkColour: MGLB.linkColour,
                    linkHover: MGLB.linkHover,
                    linkWidth: MGLB.linkWidth,
                    linkType: MGLB.linkType,
                    linkTooltipForeground: MGLB.linkTooltipForeground,
                    linkTooltipBackground: MGLB.linkTooltipBackground,
                    linkTooltipBorder: MGLB.linkTooltipBorder,
                    linkTooltip: ""
                };

                var newnode;

                // If there is already a non-label root node and we haven't
                // been dropped onto a node then we don't allow the operation
                var RN = GetDetailRootNode();
                if (RN)
                    if (myDiagram.findPartsAt(point).iterator.count == 0) {
                        SuppressSelectionMove = false;
                        return;
                    }

                myDiagram.startTransaction('new node');

                myDiagram.model.addNodeData(newdata);
                newnode = myDiagram.findNodeForData(newdata);

                if (newnode) {
                    myDiagram.select(newnode);
                    dropped(newnode, point);

                    var seq = 0;
                    var ParentNode = myDiagram.findNodeForKey(newnode.data.parent)
                    if (ParentNode) {
                        //ResequenceChildren(ParentNode);
                    } else {
                        myDiagram.isModified = true;
                        ModelChanged(true);
                    }
                }

                myDiagram.commitTransaction('new node');
                // remove the item from the AEA and then rebuild the AEA
                AEAList.splice(AEAList.findIndex(a => a.item_ref === event.dataTransfer.getData('item_ref')), 1)
                BuildAEA();

                if (!RN)
                    ZoomToFit();
            }
        }
        onHighlight(null, event);
        SuppressSelectionMove = false;

    }, false);
}
function onHighlight(part, event) { // may be null

    const oldskips = myDiagram.skipsUndoManager;
    myDiagram.skipsUndoManager = true;
    myDiagram.startTransaction("highlightcolor");
    if (part !== null) {
        myDiagram.highlight(part);
    } else {
        clearHighLight(event);
        myDiagram.clearHighlighteds();
    }
    myDiagram.commitTransaction("highlightcolor");
    myDiagram.skipsUndoManager = oldskips;
}
function dropped(newNode, point) {
    iawVerbose("dropped()");

    var a = JSON.parse(myDiagram.model.toJson()).nodeDataArray.length == 0;

    const it = myDiagram.findPartsAt(point).iterator;

    while (it.next()) {
        const part = it.value;
        if (part === newNode) continue;
        if (part && part.mouseDrop) {

            const e = new go.InputEvent();
            e.diagram = myDiagram;
            e.documentPoint = point;
            e.viewPoint = myDiagram.transformDocToView(point);
            e.up = true;
            myDiagram.lastInput = e;
            // should be running in a transaction already 
            part.mouseDrop(e, part);
            break;
        }
    }
}
function allowDrop(ev) {
    ev.preventDefault();
}
function drop(ev) {
    ev.preventDefault();
    var data = ev.dataTransfer.getData("text");
    ev.target.appendChild($ID(data));
}

//----------------------------------------------------------------------------------------------------------------------
// Left / Right Pane visibility and re-sizing
//----------------------------------------------------------------------------------------------------------------------
// Handle the mousedown event that's triggered when user drags the panel resizer
function PanelsMouseDownHandler(e) {
    // Attach the listeners to `document`
    document.addEventListener('mousemove', PanelMouseMoveHandler);
    document.addEventListener('mouseup', PanelMouseUpHandler);
};
function PanelMouseMoveHandler(e) {
    var Rect = $ID("divRight").getBoundingClientRect();
    if (e.clientX < 25 || e.clientX > Rect.right - 250) return;

    $ID("divLeft").style.width = e.clientX + 'px';
    $ID("divLeft").style.userSelect = 'none';
    $ID("divLeft").style.pointerEvents = 'none';

    $ID('dragMe').style.cursor = 'col-resize';
    document.body.style.cursor = 'col-resize';

    $ID("divRight").style.marginLeft = e.clientX + 6 + 'px';
    $ID("divRight").style.userSelect = 'none';
    $ID("divRight").style.pointerEvents = 'none';
};
function PanelMouseUpHandler() {
    iawVerbose("PanelMouseUpHandler");

    $ID('dragMe').style.removeProperty('cursor');

    document.body.style.removeProperty('cursor');
    $ID("divLeft").style.removeProperty('user-select');
    $ID("divLeft").style.removeProperty('pointer-events');
    $ID("divRight").style.removeProperty('user-select');
    $ID("divRight").style.removeProperty('pointer-events');

    // Remove the handlers of `mousemove` and `mouseup`
    document.removeEventListener('mousemove', PanelMouseMoveHandler);
    document.removeEventListener('mouseup', PanelMouseUpHandler);
};

var LeftPanelVisible = false;

function PanelHideLeft(RedrawScreen) {
    $ID("divLeft").className = "divLeft-off"
    $ID("divLeft").style.width = '0px';

    $ID("divRight").className = "divRight-full"
    $ID("divRight").style.marginLeft = '0px'
    $ID("dragMe").style.display = "none";

    $ID("btnShow").style.display = "none";
    if (MGLB.model_editable && !MGLB.narrative_model)
        $ID("btnShow").style.display = "inline-block";

    if (RedrawScreen) {
        myDiagram.requestUpdate();
        ZoomToFit();
    }
    LeftPanelVisible = false;
}
function PanelShowLeft(RedrawScreen) {
    if (LeftPanelVisible == true)
        return;
    $ID("divLeft").className = "divLeft-on"
    $ID("divLeft").style.width = '250px';
    $ID("divRight").className = "divRight-partial"
    $ID("divRight").style.marginLeft = '256px'
    $ID("btnShow").style.display = "none";
    $ID("btnHide").style.display = "inline-block";
    $ID("dragMe").style.display = "inline";

    if (RedrawScreen) {
        myDiagram.requestUpdate();
        ZoomToFit();
    }
    LeftPanelVisible = true;
}

// ------------------------------------------------------------------------------------
// Server Calls
// ------------------------------------------------------------------------------------
function GetModelParms() {

    iawVerbose("Calling GetModelParms");
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        url: "ChartService.asmx/GetModelParms",
        success: function (json) {
            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS
                }
            }
            if (json.error_message != undefined) {
                iawError("ERROR", json.error_message);
                iawError("STACK", json.error_stackTrace);
                return
            }
            iawVerbose("GetModelParms Success", json);
            MGLB = json;

            $ID("txtaea").innerHTML = "<span>" + MGLB.aea_text + "</span>";
            $ID("txtoca").innerHTML = "<span>" + MGLB.oca_text + "</span>";
            $ID("txtocadate").innerHTML = "<span>" + MGLB.chart_date + "</span>";

            // Get the table element
            const table = document.getElementById('contextMenu');

            // Get all the rows within the table
            const rows = Array.from(table.querySelectorAll('tr'));

            // Sort the rows alphabetically based on the text content in the .text-wrapper cells, taking locale into consideration
            rows.sort((a, b) =>
            {
                const textA = a.querySelector('.text-wrapper').textContent.trim();
                const textB = b.querySelector('.text-wrapper').textContent.trim();
                return textA.localeCompare(textB);
            });
            // Clear the existing rows from the table and append the sorted rows
            rows.forEach(row => table.appendChild(row));

            // Add the font elements to the ddlbfont select element
            //var fonts = json.fonts;
            var ddlb = $ID("ddlbFont");

            // Loop through the "fonts" array and do something with the font data
            for (var i = 0; i < MGLB.fonts.length; i++) {
                ddlb.add(new Option(MGLB.fonts[i].font_name));
            }

            // Set the headers for the AEA List
            var cols = MGLB.AEACols.split("|");
            var align = MGLB.AEAColsAlign.split("|");
            var sortable = MGLB.AEAColsSort.split("|");
            var i;

            $ID("AEAData").innerHTML = "";
            for (i = 0; i < cols.length; i++) {
                if (sortable[i] == "true") {
                    $ID("AEAData").innerHTML += i == 0 ?
                        '<th data-column="' + i + '" data-sortable="true" class="listheader ' + align[i] + ' nowrap cur-pointer"><span id="AEAHeadIcon' + i + '" class="IconPic Icon16 IconSort_Asc"></span>&nbsp;' + cols[i] + '</th>' :
                        '<th data-column="' + i + '" data-sortable="true" class="listheader ' + align[i] + ' nowrap cur-pointer"><span id="AEAHeadIcon' + i + '" class="IconPic Icon16 IconSortable"></span>&nbsp;' + cols[i] + '</th>';
                } else {
                    // No sorting on this column!
                    $ID("AEAData").innerHTML += '<th data-column="' + i + '"data-sortable="false" class="' + align[i] + ' nowrap">' + cols[i] + '</th>';
                }
            }

            $('th').on('click', function () {
                var column = $(this).data('column')
                var sortable = MGLB.AEAColsSort.split("|");
                if (sortable[column] == "false")
                    return;

                if (column != AEASortColumn) {
                    AEASortColumn = column;
                    AEASortAscending = true; // new column always start with sort ascending
                } else {
                    AEASortAscending = !AEASortAscending;
                }
                // Clear the current sort icon classes and set them all to Sortable except for the one clicked on
                for (let c = 0; c < $("th").length; c++) {
                    if (sortable[c] == "false")
                        continue;
                    var el = $ID("AEAHeadIcon" + c);
                    el.classList.remove("IconSortable", "IconSort_Asc", "IconSortDesc");
                    if (c != column)
                        el.classList.add("IconSortable");
                }
                // Now set the class on the clicked column to the correct Asc or Desc
                var el = $ID("AEAHeadIcon" + column);
                if (AEASortAscending)
                    el.classList.add("IconSort_Asc");
                else
                    el.classList.add("IconSortDesc");

                // BuildAEA will sort based on above, so that it is always done no matter where it is called from
                BuildAEA();
            })
            init(json);

            // load the data 
            if (MGLB.model_editable)
                GetUnallocatedItems();
            GetModelData();
            AndFinally();
        },
        error: function (xhr, status, err) {
            iawError("GetModelParms AJAX ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {
            iawVerbose("GetModelParms Complete");
        }
    });
}
function SaveModelParms() {

    var data = {
        backgroundContent: MGLB.backgroundContent,
        backgroundID: parseInt(MGLB.backgroundID),
        backgroundRepeat: MGLB.backgroundRepeat,
        backgroundType: MGLB.backgroundType,
        chartDirection: parseInt(MGLB.chartDirection),
        line_attr: MGLB.line_attr,
        node_fg: MGLB.node_fg.toUpperCase(),
        node_bg: MGLB.node_bg.toUpperCase(),
        node_border_fg: MGLB.node_border_fg.toUpperCase(),
        node_text_bg: MGLB.node_text_bg.toUpperCase(),
        node_text_bg_block: MGLB.node_text_bg_block,
        node_icon_fg: MGLB.node_icon_fg.toUpperCase(),
        node_icon_hover: MGLB.node_icon_hover.toUpperCase(),
        node_corners: MGLB.node_corners,
        node_height: parseInt(MGLB.node_height),
        node_width: parseInt(MGLB.node_width),
        node_highlight_fg: MGLB.node_highlight_fg.toUpperCase(),
        node_highlight_bg: MGLB.node_highlight_bg.toUpperCase(),
        node_highlight_border: MGLB.node_highlight_border.toUpperCase(),
        node_tt_fg: MGLB.node_tt_fg.toUpperCase(),
        node_tt_bg: MGLB.node_tt_bg.toUpperCase(),
        node_tt_border: MGLB.node_tt_border.toUpperCase(),
        //note_fg: MGLB.note_fg.toUpperCase(),
        //note_bg: MGLB.note_bg.toUpperCase(),
        //note_border_fg: MGLB.note_border_fg.toUpperCase(),
        show_photos: MGLB.show_photos,
        image_position: MGLB.image_position,
        image_shape: MGLB.image_shape,
        image_height: MGLB.image_height,
        showShadow: MGLB.showShadow,
        shadowColour: MGLB.shadowColour.toUpperCase(),
        linkColour: MGLB.linkColour.toUpperCase(),
        linkHover: MGLB.linkHover.toUpperCase(),
        linkType: MGLB.linkType,
        linkWidth: parseFloat(MGLB.linkWidth),
        linkTooltipForeground: MGLB.linkTooltipForeground,
        linkTooltipBackground: MGLB.linkTooltipBackground,
        linkTooltipBorder: MGLB.linkTooltipBorder,
        button_position: MGLB.button_position,
        button_font: MGLB.button_font,
        button_shape: MGLB.button_shape,
        button_text_colour: MGLB.button_text_colour,
        button_back_colour: MGLB.button_back_colour,
        button_border_colour: MGLB.button_border_colour,
        button_text_hover: MGLB.button_text_hover,
        button_back_hover: MGLB.button_back_hover,
        button_border_hover: MGLB.button_border_hover,
        button_detail_text: MGLB.button_detail_text,
        button_note_text: MGLB.button_note_text
    };
    var strdata = JSON.stringify(data);
    iawVerbose("Calling SaveModelParms");
    iawVerbose(data);

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            ModelParms: strdata
        }),
        url: "ChartService.asmx/SaveModelParms",
        success: function (json) {
            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS 
                }
            }

            if (json.error_message != undefined) {
                iawError("ERROR", json.error_message);
                iawError("STACK", json.error_stackTrace);
                return
            }
            iawVerbose("SaveModelParms Success");
            iawVerbose(json);
        },
        error: function (xhr, status, err) {
            iawError("SaveModelParms AJAX ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {

            iawVerbose("SaveModelParms Complete");
        }
    });
    // window.location.reload();
    // myDiagram.isModified = false;
    ModelChanged(true);
}
function GetUnallocatedItems() {

    // clear any existing unallocated

    $ID("ulUnallocated").innerHTML = "";

    // Reteive a list of everyone we can see who isn't already on the model

    iawVerbose("Calling GetUnallocatedItems");
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        url: "ChartService.asmx/GetUnallocatedItems",
        success: function (json) {
            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS
                }
            }
            if (json.error_message != undefined) {
                iawError("ERROR", json.error_message);
                iawError("STACK", json.error_stackTrace);
                return
            }
            iawVerbose("GetUnallocatedItems Success");
            iawVerbose(JSON.parse(json));

            AEAList = JSON.parse(json).sort(GetSortOrder("sort_name"));
        },
        error: function (xhr, status, err) {
            iawError("GetUnallocatedItems ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {
            iawVerbose("GetUnallocatedItems Complete");
        }
    });
}
function GetUnallocatedDetail(item_ref) {
    //var data = "{'ItemRef': '" + ItemRef + "' }";

    // Reteive a list of everyone we can see who isn't already on the model

    iawVerbose("Calling GetUnallocatedDetail", item_ref);
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            ItemRef: item_ref,
        }),
        async: false,
        url: "ChartService.asmx/GetUnallocatedDetail",
        success: function (json) {
            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS
                }
            }
            if (json.error_message != undefined) {
                iawError("ERROR", json.error_message);
                iawError("STACK", json.error_stackTrace);
                return
            }
            iawVerbose("GetUnallocatedDetail Success");
            iawVerbose(json);

            let arraydata = JSON.parse(json);

            // will only be one row
            arraydata.map((item, key) => {
                AEAList.push({
                    item_ref: item.item_ref,
                    name: item.name,
                    sort_name: item.sort_name
                })
            });
        },
        error: function (xhr, status, err) {
            iawError("GetUnallocatedDetail ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {
            iawVerbose("GetUnallocatedDetail Complete");
        }
    });

}
function GetModelData() {
    arr = [];
    iawVerbose("Calling GetModelData");

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        url: "ChartService.asmx/GetModelData",
        success: function (json) {
            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS
                }
            }
            if (json.error_message != undefined) {
                iawError("ERROR", json.error_message);
                iawError("STACK", json.error_stackTrace);
                return
            }
            iawVerbose("GetModelData Success");
            iawVerbose(JSON.parse(json));

            let arraydata = JSON.parse(json);
            linkArr = []; // reset the link array
            JSON.parse(json).map((item, index) => {

                if (item.line_attr != "") {
                    item.line_attr = JSON.parse(item.line_attr);
                }
                if (item.line_attr == "") {
                    item.line_attr = MGLB.line_attr;
                }

                // extract the display attributes from the line_attr value
                var attr = GetLineAttrs(item.line_attr);
                var groupKey = 0;
                if (item.parent_key > 0 && item.co_child == true) {
                    var parentgroup = arraydata.filter(element => element.detail_key == item.parent_key &&
                        element.node_type == "ParentGroup");
                    if (parentgroup != null) {
                        groupKey = item.parent_key;
                    }
                }

                arr.push({
                    key: item.detail_key,
                    sequence: item.sequence,
                    parent: item.parent_key,
                    line1: item.line1,
                    line2: item.line2,
                    line3: item.line3,
                    line4: item.line4,
                    line5: item.line5,
                    line6: item.line6,
                    ds_line1: item.ds_line1, // store original values so that when we come to send them
                    ds_line2: item.ds_line2, // back to the server, we only want to save them if they have 
                    ds_line3: item.ds_line3, // changed.  when we load the data back from the database on 
                    ds_line4: item.ds_line4, // subsequent calls, we will use the changed values in preference
                    ds_line5: item.ds_line5, // to the data from the datasource
                    ds_line6: item.ds_line6,

                    line_attr: item.line_attr,

                    font1: attr.font[1],
                    font2: attr.font[2],
                    font3: attr.font[3],
                    font4: attr.font[4],
                    font5: attr.font[5],
                    font6: attr.font[6],

                    isUnderline1: attr.isUnderline[1],
                    isUnderline2: attr.isUnderline[2],
                    isUnderline3: attr.isUnderline[3],
                    isUnderline4: attr.isUnderline[4],
                    isUnderline5: attr.isUnderline[5],
                    isUnderline6: attr.isUnderline[6],

                    fontColour1: attr.colour[1],
                    fontColour2: attr.colour[2],
                    fontColour3: attr.colour[3],
                    fontColour4: attr.colour[4],
                    fontColour5: attr.colour[5],
                    fontColour6: attr.colour[6],

                    fontBgColour1: attr.bg_colour[1],
                    fontBgColour2: attr.bg_colour[2],
                    fontBgColour3: attr.bg_colour[3],
                    fontBgColour4: attr.bg_colour[4],
                    fontBgColour5: attr.bg_colour[5],
                    fontBgColour6: attr.bg_colour[6],

                    alignment1: attr.align[1],
                    alignment2: attr.align[2],
                    alignment3: attr.align[3],
                    alignment4: attr.align[4],
                    alignment5: attr.align[5],
                    alignment6: attr.align[6],

                    item_ref: item.item_ref,
                    tooltip: item.tooltip,
                    show_detail: item.show_detail,
                    location: new go.Point(item.pos_x, item.pos_y),
                    loc: item.pos_x + " " + item.pos_y,
                    isAssistant: item.assistant,
                    source: item.node_picture,
                    visible: MGLB.show_photos == true && MGLB.photos_applicable == true && item.show_photo,
                    photoshow: MGLB.show_photos == true && MGLB.photos_applicable == true && item.show_photo,
                    image_height: item.image_height,
                    success: "true",
                    node_fg: item.node_fg,
                    node_bg: item.node_bg,
                    node_border_fg: item.node_border_fg,
                    node_text_bg: item.node_text_bg,
                    node_text_bg_block: item.node_text_bg_block,
                    node_icon_fg: item.node_icon_fg,
                    node_icon_hover: item.node_icon_hover,
                    node_width: item.node_width == null ? parseInt(MGLB.node_width) : parseInt(item.node_width),
                    node_height: item.node_height == null ? parseInt(MGLB.node_height) : parseInt(item.node_height),
                    node_corners: item.node_corners.trim() != "" ? item.node_corners.trim() : item.node_type == 'Label' ? 'Circle' : MGLB.node_corners,
                    isTreeExpanded: item.tree_expanded,
                    isSubGraphExpanded: item.node_type !== "ParentGroup" ? true : (item.group_expanded === undefined || item.group_expanded === null ? true : item.group_expanded),
                    node_type: item.node_type,
                    node_tt_fg: item.node_tt_fg,
                    node_tt_bg: item.node_tt_bg,
                    node_tt_border: item.node_tt_border,
                    label_text: item.label_text,
                    label_icon: item.label_icon,
                    private_label: item.private_label,
                    individualphotoshow: item.show_photo,
                    existnode: 1,
                    dragtype: "oca",
                    margin: new go.Margin(0, 5, 0, 5),
                    info: "i",
                    isNote: item.label_text == "" ? false : true,
                    isCoParent: item.co_child,
                    picture_width: parseInt(item.picture_width),
                    node_table_width: CalcNodeTableWidth(item.node_type,
                        MGLB.show_photos == true && MGLB.photos_applicable == true && item.show_photo == true,
                        item.node_width == null ? parseInt(MGLB.node_width) : parseInt(item.node_width),
                        parseInt(item.picture_width)),
                    isGroup: item.node_type == "ParentGroup" ? true : false,
                    group: groupKey,
                    category: item.node_type == "Label" ? "Label" : item.node_type == "ParentGroup" ? 'ParentGroup' : '',
                    columns: item.node_type == 'ParentGroup' ? item.group_count : 0,
                    nodes_across: item.nodes_across,
                    showShadow: item.showShadow,
                    shadowColour: item.shadowColor,
                    linkColour: item.linkColour,
                    linkHover: item.linkHover,
                    linkWidth: item.linkWidth,
                    linkType: item.linkType,
                    linkTooltipForeground: item.linkTooltipForeground,
                    linkTooltipBackground: item.linkTooltipBackground,
                    linkTooltipBorder: item.linkTooltipBorder,
                    linkTooltip: item.linkTooltip
                });
                if (groupKey == 0) {
                    linkArr.push({
                        from: item.parent_key,
                        to: item.detail_key,
                    });
                }
                groupKey = 0
            });
            var newobj = {
                "class": "go.GraphLinksModel",
                nodeDataArray: arr,
                linkDataArray: linkArr
            }
            myDiagram.model = go.Model.fromJson(newobj);

            var lastkey = 1;
            myDiagram.model.makeUniqueKeyFunction = (model, data) => {
                var k = data.key || lastkey;
                while (model.findNodeDataForKey(k)) k++;
                data.key = lastkey = k;
                return k;
            };
        },
        error: function (xhr, status, err) {
            iawError("GetModelData AJAX ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {
            iawVerbose("GetModelData Complete");
        }
    });
}
function GetModelDataItem(item_ref, node_height) {

    //var data = "{'ItemRef': '" + ItemRef + "','nodeHeight': " + node_height + " }";
    var result = null;
    iawVerbose("Calling GetModelDataItem", item_ref);
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            ItemRef: item_ref,
            nodeHeight: node_height
        }),
        async: false,
        url: "ChartService.asmx/GetModelDataItem",
        success: function (json) {
            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS
                }
            }
            if (json.error_message != undefined) {
                iawError("ERROR : " + json.error_message);
                iawError("STACK : " + json.error_stackTrace);
                return
            }
            iawVerbose("GetModelDataItem Success");
            iawVerbose(json);

            let arraydata = JSON.parse(json);
            if (arraydata.length > 0) {
                result = arraydata[0];
            }
        },
        error: function (xhr, status, err) {
            iawError("GetModelDataItem AJAX ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {
            iawVerbose("GetModelDataItem Complete");
        }
    });

    return result;
}
function GetModelDataItemForType(item_type, item_ref, node_height) {
    //var data = "{'ItemRef': '" + ItemRef + "','nodeHeight': " + node_height + " }";
    var result = null;
    iawVerbose("Calling GetModelDataItemForType", item_type, item_ref);
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            RequiredNodeType: item_type,
            ItemRef: item_ref,
            nodeHeight: node_height
        }),
        async: false,
        url: "ChartService.asmx/GetModelDataItemForType",
        success: function (json) {
            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS
                }
            }
            if (json.error_message != undefined) {
                iawError("ERROR : " + json.error_message);
                iawError("STACK : " + json.error_stackTrace);
                return
            }
            iawVerbose("GetModelDataItem Success");
            iawVerbose(json);

            let arraydata = JSON.parse(json);
            if (arraydata.length > 0) {
                result = arraydata[0];
            }
        },
        error: function (xhr, status, err) {
            iawError("GetModelDataItemForType AJAX ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {
            iawVerbose("GetModelDataItemForType Complete");
        }
    });

    return result;
}
function SaveModelData(ModelData) {
    myDiagram.isModified = false;

    iawVerbose("Calling SaveModelData");

    var strdata = JSON.stringify(ModelData);

    iawVerbose(ModelData);
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            ModelData: strdata
        }),
        url: "ChartService.asmx/SaveModelData",
        success: function (json) {

            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS
                }
            }
            if (json.error_message != undefined) {
                iawError("ERROR", json.error_message);
                iawError("STACK", json.error_stackTrace);
                return
            }
            iawVerbose("SaveModelData Success");
            iawVerbose(json);
        },
        error: function (xhr, status, err) {
            iawError("SaveModelData AJAX ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {
            iawVerbose("SaveModelData Complete");
        }
    });
}
function GetNodePicture(item_ref, node_height) {
    //var data = "{'PayRef': '" + PayRef + "','nodeHeight': " + node_height + " }";
    var result = null;
    iawVerbose("Calling GetNodePicture", item_ref);
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            ItemRef: item_ref,
            nodeHeight: node_height
        }),
        async: false,
        url: "ChartService.asmx/GetNodePicture",
        success: function (json) {
            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS
                }
            }
            if (json.error_message != undefined) {
                iawError("ERROR : " + json.error_message);
                iawError("STACK : " + json.error_stackTrace);
                return
            }
            iawVerbose("GetNodePicture Success");
            iawVerbose(json);

            result = json;
        },
        error: function (xhr, status, err) {
            iawError("GetNodePicture AJAX ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {
            iawVerbose("GetNodePicture Complete");
        }
    });

    return result;
}
function GetDetailInfo(item_ref) {
    //var data = "{'ItemRef': '" + PayRef + "' }";
    var result = null;
    iawVerbose("Calling GetDetailInfo", item_ref);
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            ItemRef: item_ref,
        }),
        async: false,
        url: "ChartService.asmx/GetDetailInfo",
        success: function (json) {
            if (!$.isEmptyObject(json)) {
                if (json.hasOwnProperty("d")) {
                    json = json.d; // asp.net 3.5 XSS
                }
            }
            if (json.error_message != undefined) {
                iawError("ERROR", json.error_message);
                iawError("STACK", json.error_stackTrace);
                return
            }
            iawVerbose("GetDetailInfo Success");
            iawVerbose(json);

            result = json.html;
        },
        error: function (xhr, status, err) {
            iawError("GetDetailInfo AJAX ERROR", err + " " + xhr.status);
        },
        complete: function (xhr, status) {
            iawVerbose("GetDetailInfo Complete");
        }
    });
    return result;
}
function KeepSessionAlive() {

    iawVerbose("Calling KeepAlive");
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: "ChartService.asmx/KeepAlive",
        success: function (json) {
            iawVerbose("KeepAlive Success");
            restartTimers();
            setTimeout(KeepSessionAlive, 5 * 60 * 1000); // every 5 mins
        },
        error: function (xhr, status, err) {
            iawError("KeepAlive AJAX ERROR", err + " " + xhr.status);
            return "";
        },
        complete: function (xhr, status) {
            iawVerbose("KeepAlive Complete");
        }
    });
}

//-----------------------------------------------------------------------------------------------
// Model Buttons
//-----------------------------------------------------------------------------------------------
var temp = false; // what ?   'temp'  
var actiontype = ""
var typenode = "NA";
var myOverview = null;

function addModelButtonEvents() {
    // Add the click events for the page level buttons

    $ID("btnZoomFit").addEventListener("click", (event) => {
        ZoomToFit();
    });
    $ID("btnCentreTop").addEventListener("click", (event) => {
        CenterToTop();
    });
    $ID("btnCentreRootNode").addEventListener("click", (event) => {
        CenterToRootNode();
    });
    $ID("btnChangeDirection").addEventListener("click", (event) => {
        changeLayout();
    });
    $ID("btnShowMagnifier").addEventListener("click", (event) => {
        ShowMagnifier(event);
    });
    $ID("btnDownloadPDF").addEventListener("click", (event) => {
        downloadPdf(event);
    });
    $ID("btnModelSearch").addEventListener("click", (event) => {
        ModelSearch(event);
    });
    $ID("btnModelSearchClear").addEventListener("click", (event) => {
        clearHighLight(event);
    });
    $ID("btnModelSave").addEventListener("click", (event) => {
        ModelSave(event);
    });
    $ID("txtSearchModel").addEventListener("keyup", function (event) {
        if (event.keyCode == 13) {
            ModelSearch(event);
            event.preventDefault();
        }
    });
    $ID("txtSearchModel").addEventListener("focus", function (event) {
        $ID("txtSearchModel").value = "";
        if (myDiagram.allowUndo == true) {
            clearHighLight(event);
        }
    });

    ModelChanged(false);
}
function confirmUndo() {
    var confirmMessage = document.getElementById('<%= ltlConfirmUndo.ClientID %>').innerText;
    return confirm(confirmMessage);
}
function ModelChanged(flag) {
    if (!MGLB)
        flag = false;
    else
        if (MGLB.model_editable == false)
            flag = false;

    if (flag == true) {
        BannerEnabled(false);
        $ID("btnModelSave").style.display = "inline-block";
        $ID("btnModelUndo").style.display = "inline-block";
        $ID("btnFirstChart").style.display = "none";
        $ID("btnEarlierChart").style.display = "none";
        $ID("btnLaterChart").style.display = "none";
        $ID("btnLastChart").style.display = "none";
        $ID("btnModelBackToList").style.display = "none";
    } else {
        BannerEnabled(true);
        $ID("btnModelSave").style.display = "none";
        $ID("btnModelUndo").style.display = "none";
        $ID("btnFirstChart").style.display = "inline-block";
        $ID("btnEarlierChart").style.display = "inline-block";
        $ID("btnLaterChart").style.display = "inline-block";
        $ID("btnLastChart").style.display = "inline-block";
        $ID("btnModelBackToList").style.display = "inline-block";
    }
}
function ModelChangedSearch(flag) {
    if (flag == true)
        $ID("btnModelSearchClear").style.display = "inline-block";
    else
        $ID("btnModelSearchClear").style.display = "none";
}
function clearHighLight(event) {
    event.preventDefault();
    $ID("txtSearchModel").value = "";
    myDiagram.clearHighlighteds();
    ModelChangedSearch(false);

}
function ModelSearch(event) {
    event.preventDefault();
    //filterdata = [];
    var input = $ID("txtSearchModel");
    if (!input) return;
    myDiagram.focus();
    myDiagram.startTransaction("highlight search");
    if (input.value) {
        var safe = input.value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        var regex = new RegExp(safe, "i");
        var nodeResults = myDiagram.findNodesByExample({
            line1: regex
        }, {
            line2: regex
        }, {
            line3: regex
        }, {
            line4: regex
        }, {
            line5: regex
        }, {
            line6: regex
        });
        var results = new go.List();
        results.addAll(nodeResults);

        myDiagram.findTopLevelGroups().each(function (g) {
            if ((g.data.line1 && regex.test(g.data.line1)) ||
                (g.data.line2 && regex.test(g.data.line2)) ||
                (g.data.line3 && regex.test(g.data.line3)) ||
                (g.data.line4 && regex.test(g.data.line4)) ||
                (g.data.line5 && regex.test(g.data.line5)) ||
                (g.data.line6 && regex.test(g.data.line6))) {
                results.add(g);
            }
        });
        myDiagram.highlightCollection(results);

        // try to center the diagram at the first node that was found
        if (results.count > 0) {
            ModelChangedSearch(true);
            actiontype = "search";
            myDiagram.centerRect(results.first().actualBounds);
            myDiagram.nodes.filter(node => node.isHighlighted == true).each(node => {
                // for each found node, go up the tree ensuring each is expanded
                node.findTreeParentChain().each(nodelist => {
                    if (nodelist.data.key != node.data.key) {
                        if (nodelist.isTreeExpanded === false) {
                            nodelist.isTreeExpanded = true;
                            myDiagram.isModified = true;

                            ModelChanged(true);
                        }
                    }
                });
            })
        }
    } else { // empty string only clears highlighteds collection

        myDiagram.clearHighlighteds();
        ModelChangedSearch(false);
        return false;
    }

    myDiagram.commitTransaction("highlight search");

    ZoomToFit();
    return false;
}
function ModelSave(event) {
    event.preventDefault();

    window.onbeforeunload = null;

    var datanew = myDiagram.model.toJSON();
    var shiftArr_Front = JSON.parse(datanew);
    let savedata = [];

    shiftArr_Front.nodeDataArray.map((item) => {

        var lines = [null, item.line1, item.line2, item.line3, item.line4, item.line5, item.line6];
        var ds_lines = [null, item.ds_line1, item.ds_line2, item.ds_line3, item.ds_line4, item.ds_line5, item.ds_line6];

        if (item.node_type == "Detail") {
            for (var i = 1; i <= lines.length - 1; i++) {
                lines[i] = lines[i] == ds_lines[i] ? null : lines[i];
            }
        }

        savedata.push({
            detail_key: item.key,
            parent_key: item.parent,
            item_ref: item.item_ref,
            pos_x: item.location.x,
            pos_y: item.location.y,
            node_width: parseInt(item.node_width),
            node_height: parseInt(item.node_height),
            node_corners: item.node_corners,
            assistant: item.isAssistant,
            node_fg: item.node_fg,
            node_bg: MGLB.node_bg.toUpperCase() != item.node_bg.toUpperCase() ? item.node_bg.toUpperCase() : "" || item.node_bg.toUpperCase() == undefined ? MGLB.node_bg.toUpperCase() : item.node_bg.toUpperCase(),
            node_border_fg: item.node_border_fg,
            node_text_bg: item.node_text_bg,
            node_text_bg_block: item.node_text_bg_block,
            node_icon_fg: item.node_icon_fg,
            node_icon_hover: item.node_icon_hover,
            show_photo: item.individualphotoshow == true ? true : item.photoshow,
            image_height: item.image_height,
            tree_expanded: item.isTreeExpanded == undefined ? true : item.isTreeExpanded,
            group_expanded: item.isSubGraphExpanded === undefined ? true : item.isSubGraphExpanded,
            node_type: item.node_type,
            line1: lines[1],
            line2: lines[2],
            line3: lines[3],
            line4: lines[4],
            line5: lines[5],
            line6: lines[6],
            node_tt_fg: item.node_tt_fg == undefined ? MGLB.node_tt_fg : item.node_tt_fg,
            node_tt_bg: item.node_tt_bg == undefined ? MGLB.node_tt_bg : item.node_tt_bg,
            node_tt_border: item.node_tt_border == undefined ? MGLB.node_tt_border : item.node_tt_border,
            tooltip: item.tooltip == undefined ? "" : item.tooltip,
            label_text: item.label_text == undefined ? "" : item.label_text,
            label_icon: item.label_icon == undefined ? "" : item.label_icon,
            private_label: item.private_label,
            sequence: item.sequence,
            line_attr: item.line_attr,
            co_child: item.isCoParent,
            nodes_across: item.nodes_across,
            showShadow: item.showShadow,
            shadowColour: item.shadowColour,
            linkColour: item.linkColour,
            linkHover: item.linkHover,
            linkWidth: parseFloat(item.linkWidth),
            linkType: item.linkType,
            linkTooltipForeground: item.linkTooltipForeground,
            linkTooltipBackground: item.linkTooltipBackground,
            linkTooltipBorder: item.linkTooltipBorder,
            linkTooltip: item.linkTooltip
        })
    })
    iawVerbose("sending data", savedata);
    SaveModelData(savedata);
    SaveModelParms();

    ModelChanged(false);
    return false;
}
function ModelCancel(event) {
    event.preventDefault();
    myDiagram.div = null;
    GetModelParms();

    ModelChanged(false);
    return false;
}
function ShowMagnifier() {
    // add code to initialise the magnifier control
    myDiagram.focus();

    var myOverviewDiv = $ID("myOverviewDiv");

    if (myOverview === null || myOverviewDiv.style.display === "none") {
        myOverviewDiv.style.display = "inline";
        myOverviewDiv.style.left = null;
        myOverviewDiv.style.right = "0px";
        myOverviewDiv.style.top = "0px";
        // myOverviewDiv.style.backgroundColor = getComputedStyle(document.documentElement).getPropertyValue('--content-area-bg-color').trim();
        myOverviewDiv.style.backgroundColor = MGLB.backgroundContent;
        if (myOverview !== null) {
            myOverview.observed = null;
            return;
        }

        // create Overview
        myOverview =
            go.GraphObject.make(go.Overview, myOverviewDiv, {
                scrollMode: go.Diagram.InfiniteScroll,
                "box.visible": false,
                observed: myDiagram, // tell it which Diagram to show
                // disable normal Overview functionality to make it act as a magnifying glass:
                initialScale: 1, // zoom in even more than normal
                autoScale: go.Diagram.None, // don't show whole observed Diagram
                hasHorizontalScrollbar: false, // don't show any scrollbars
                hasVerticalScrollbar: false
            });

        // implement the magnifying glass functionality, to have the Overview show part of the Diagram where the mouse is
        myDiagram.toolManager.doMouseMove = function () {
            go.ToolManager.prototype.doMouseMove.call(myDiagram.toolManager);

            myOverview.observed = myDiagram;

            var myOverviewDiv = $ID("myOverviewDiv");

            if (myOverviewDiv.style.display !== "none") {
                var e = myDiagram.lastInput;
                var osize = myOverview.viewportBounds.size;
                myOverview.position = new go.Point(e.documentPoint.x - osize.width / 2, e.documentPoint.y - osize.height / 2);
                myOverviewDiv.style.left = (e.viewPoint.x - myOverviewDiv.scrollWidth / 2) + "px";
                myOverviewDiv.style.top = (e.viewPoint.y - myOverviewDiv.scrollHeight / 2) + "px";
            }
        };

        // implement the magnifying glass functionality, to have the Overview show part of the Diagram where the mouse is
        myDiagram.toolManager.doMouseDown = function () {
            go.ToolManager.prototype.doMouseDown.call(myDiagram.toolManager);
            var clickedPart = myDiagram.findPartAt(myDiagram.lastInput.documentPoint, false);
            if (clickedPart instanceof go.Node) {
                return;
            }
            var myOverviewDiv = $ID("myOverviewDiv");
            if (myOverviewDiv.style.display !== "none") {
                // hide DIV
                myOverviewDiv.style.display = "none";
            }
        };
    } else {
        // hide DIV
        myOverviewDiv.style.display = "none";
    }
}

//----------------------------------------------------------------------------------------------------------------------
// Context Menus
//----------------------------------------------------------------------------------------------------------------------
function ContextMenuCommand(event, val) {
    if (val === undefined) val = event.currentTarget.id;
    var diagram = myDiagram;

    switch (val) {
        case "make_group":
            AddTeamNode(diagram.selection.first());
            break;
        case "node_setting":
            NodeSettingsForm(diagram.selection.first().data);
            break;
        case "assistant_on":
            ToggleAssitant1(diagram.selection.first());
            break;
        case "assistant_off":
            ToggleAssitant1(diagram.selection.first());
            break;
        case "make_vacant":
            MakeVacant(diagram.selection.first());
            break;
        case "modal_setting":
            ModelSettingsForm();
            break;
        case "add_text":
            AddLabelNode();
            break;
        case "move_left":
            SwitchNode(diagram.selection.first(), "Left");
            break;
        case "move_right":
            SwitchNode(diagram.selection.first(), "Right");
            break;
        case "label_detail":
            GetNodeInfoLabel(diagram.selection.first());
            break;
        case "make_parent_group":
            MakeParentGroup(diagram.selection.first());
            break;
        case "make_new_parent":
            MakeNewParent(diagram.selection.first());
            break;
        case "make_child":
            MakeChild(diagram.selection.first());
            break;
        case "move_down_to_parent_group":
            MoveToParentGroup(diagram.selection.first());
            break;
        case "move_up_to_parent_group":
            MoveUpToParentGroup(diagram.selection.first());
            break;
        case "link_settings":
            LinkSettingsForm(diagram.selection.first());
            break;
        case "expand_below":
            expandFrom(diagram.selection.first())
            break;
        case "collapse_below":
            collapseFrom(diagram.selection.first())
            break;
        case "delete_parent_group":
            deleteNode(diagram.selection.first())
            break;
        //case "node_detail":
        //    GetNodeInfo(diagram.selection.first());
        //    break;
        //case "zoom":
        //    ZoomToFit(diagram);
        //    break;
        //case "center":
        //    CenterToTop(diagram);
        //    break;
        //case "center_root_node":
        //    CenterToRootNode();
        //    break;
    }
}
function GetNodeInfoLabel(node) {
    $ID("divItemDetail").innerHTML = node.data.label_text;
    $ID("divItemDetail").style.width = (node.data.note_width - 35) + "px";
    $find('mpeItemDetailForm').show();

    return false;
}
function GetToolTipText(node) {
    $ID("divItemDetail").innerHTML = node.data.tooltip;
    const collection = $ID('divItemDetail').children;

    var n = collection.length;
    for (let i = 0; i < n; i++) {

        n = collection.length;

        collection[i].style.color = node.data.node_tt_fg == "" ? MGLB.node_tt_fg : node.data.node_tt_fg; 
        collection[i].style.margin = 0;
    }

    $ID("divItemDetail").style.color = node.data.node_tt_fg == "" ? MGLB.node_tt_fg : node.data.node_tt_fg;
    $ID("divItemDetail").style.backgroundColor = node.data.node_tt_bg == "" ? MGLB.node_tt_bg : node.data.node_tt_bg;
    $ID("divItemDetail").style.borderColor = node.data.node_tt_border == "" ? MGLB.node_tt_border : node.data.node_tt_border;
    $find('mpeItemDetailForm').show();
    return false;
}
function ZoomToFit() {
    iawVerbose("ZoomToFit()");

    myDiagram.commandHandler.zoomToFit();
    myDiagram.commandHandler.isZoomToFitRestoreEnabled = false;

    AlignDoc();
}
function CenterToTop(node) {
    iawVerbose("CenterToTop()");

    myDiagram.isModified = false;
    myDiagram.scale = 1;

    AlignDoc();

    $ID("btnCentreTop").style.display = "none";
    $ID("btnCentreRootNode").style.display = "inline-block";
}
function CenterToRootNode() {
    iawVerbose("CenterToRootNode()");
    AlignDoc();
    myDiagram.isModified = false;
    myDiagram.scale = 1;

    var node = GetDetailRootNode();
    if (node != null) {
        var vb = myDiagram.viewportBounds;

        switch (MGLB.chartDirection) {
            case LeftToRight:
                myDiagram.position = new go.Point(node.actualBounds.left - myDiagram.padding.left, node.actualBounds.centerY - vb.height / 2);
                break;
            case TopDown:
                myDiagram.position = new go.Point(node.actualBounds.centerX - vb.width / 2, node.actualBounds.top - myDiagram.padding.top);
                break;
            case RightToLeft:
                myDiagram.position = new go.Point(node.actualBounds.right - vb.width + myDiagram.padding.right, node.actualBounds.centerY - vb.height / 2);
                break;
            case BottomUp:
                myDiagram.position = new go.Point(node.actualBounds.centerX - vb.width / 2, node.actualBounds.bottom - vb.height + myDiagram.padding.bottom);
                break;
        }

        $ID("btnCentreRootNode").style.display = "none";
        $ID("btnCentreTop").style.display = "inline-block";
    }
}
function AlignDoc() {
    var align = go.Spot.Top;
    switch (MGLB.chartDirection) {
        case LeftToRight:
            align = go.Spot.Left;
            break;
        case TopDown:
            align = go.Spot.Top;
            break;
        case RightToLeft:
            align = go.Spot.Right;
            break;
        case BottomUp:
            align = go.Spot.Bottom;
            break;
    }
    myDiagram.alignDocument(align, align);
}
function MenuRightAlignInColumn() {
    iawVerbose("MenuRightAlignInColumn()");
    myDiagram.nodes.each(function (node) {
        var showIconTxt = node.findObject("SHOW_DETAILS_ICON_TEXT");
        if (showIconTxt != null) {
            showIconTxt.alignment = go.Spot.TopRight;
            showIconTxt.margin = new go.Margin(12, 2, 0, 0);
            showIconTxt.font = "14pt IAWSpecial";
        }
        var noteIconTxt = node.findObject("NOTE_ICON_TEXT");
        if (noteIconTxt != null) {
            noteIconTxt.alignment = go.Spot.TopRight;
            noteIconTxt.margin = new go.Margin(34, 3, 0, 0);
            noteIconTxt.font = "14pt IAWSpecial";
        }
    });
}
function MenuRightAlignInRow() {
    iawVerbose("MenuRightAlignInRow()");
    myDiagram.nodes.each(function (node) {
        var showIconTxt = node.findObject("SHOW_DETAILS_ICON_TEXT");
        if (showIconTxt != null) {
            showIconTxt.alignment = go.Spot.BottomRight;
            showIconTxt.margin = new go.Margin(0, 25, -2, 0);
            showIconTxt.font = "12pt IAWSpecial";
        }
        var noteIconTxt = node.findObject("NOTE_ICON_TEXT");
        if (noteIconTxt != null) {
            noteIconTxt.alignment = go.Spot.BottomRight;
            noteIconTxt.margin = new go.Margin(0, 44, -2, 0);
            noteIconTxt.font = "12pt IAWSpecial";
        }
    });
}
function getRadioValue(name) {
    var radio = document.getElementsByName(name);
    for (var i = 0; i < radio.length; i++)
        if (radio[i].checked) return radio[i].value;
}
function getSelectBoxValue(name) {
    var dropdownList = $ID(name);
    var value = dropdownList.options[dropdownList.selectedIndex].value;
    return value;
}
function changeLayout() {
    myDiagram.scale = 1;

    switch (MGLB.chartDirection) {
        case TopDown:
            MGLB.chartDirection = RightToLeft;
            break;
        case RightToLeft:
            MGLB.chartDirection = BottomUp;
            break;
        case BottomUp:
            MGLB.chartDirection = LeftToRight;
            break;
        case LeftToRight:
            MGLB.chartDirection = TopDown;
            break;
        default:
            MGLB.chartDirection = TopDown;
            break;
    }
    myDiagram.layout.angle = MGLB.chartDirection;
    myDiagram.layout.alternateAngle = MGLB.chartDirection;

    myDiagram.rebuildParts();
    myDiagram.requestUpdate();

    ZoomToFit();

    ModelChanged(true);

    ZoomToFit();
}
function ToggleAssitant1(node) {

    if (node !== null) {
        myDiagram.startTransaction("toggle assistant");
        var ParentNode = node.findTreeParentNode();

        if (node.data.isAssistant) {
            // we are making the node a normal child
            var NextSeq = ResequenceChildren(ParentNode);
            SetProperty(node.data, "isAssistant", false);
            SetProperty(node.data, "sequence", NextSeq);
            ResequenceAssistants(ParentNode);
        } else {
            // we are making the node am assistant
            var NextSeq = ResequenceAssistants(ParentNode);
            SetProperty(node.data, "isAssistant", true);
            SetProperty(node.data, "sequence", NextSeq);
            ResequenceChildren(ParentNode);
        }
        console.log("node.data", node.data.isAssistant);
        myDiagram.layoutDiagram(true);
        myDiagram.commitTransaction("toggle assistant");

    }

    ModelChanged(true);
}
function MakeVacant(node) {

    if (node !== null) {
        var thisNode = node.data;

        if (thisNode.node_type == "Detail") {
            GetUnallocatedDetail(thisNode.item_ref);
            BuildAEA();
        }

        myDiagram.startTransaction("vacate");
        SetProperty(thisNode, "name", MGLB.vacant_text);
        SetProperty(thisNode, "line1", MGLB.vacant_text);
        SetProperty(thisNode, "item_ref", "");
        SetProperty(thisNode, "tooltip", "");
        SetProperty(thisNode, "node_type", "Vacant");
        SetProperty(thisNode, "photoshow", false);
        SetProperty(thisNode, "node_table_width", CalcNodeTableWidth('Vacant', false, thisNode.node_width, 0));
        SetProperty(thisNode, "show_detail", false);

        myDiagram.commitTransaction('vacate');
    }
    ModelChanged(true);
}
function GetNodeInfo(node) {
    if (node.data.node_type == "Detail") {
        $ID("divItemDetail").innerHTML = GetDetailInfo(node.data.item_ref);
        $ID("divItemDetail").style.width = 'auto';
        $find('mpeItemDetailForm').show();
    }
    return false;
}
function SwitchNode(node, Direction) {
    var otherNode;
    var LR = checkNodeLR(node);
    if (Direction == 'Left' && !LR.canLeft) return;
    if (Direction == 'Right' && !LR.canRight) return;

    // work out which 'other' node we're interested in, left or right depending on direction
    if (Direction == 'Left')
        otherNode = myDiagram.findNodeForKey(LR.prevKey);
    else
        otherNode = myDiagram.findNodeForKey(LR.nextKey);

    var seq1 = node.data.sequence;
    var seq2 = otherNode.data.sequence;

    node.data.sequence = seq2;
    otherNode.data.sequence = seq1;

    myDiagram.startTransaction("SetSeq");
    SetProperty(node.data, "sequence", seq2);
    SetProperty(otherNode.data, "sequence", seq1);
    myDiagram.commitTransaction("SetSeq");

    // Get the parent group of the node
    //if (node.data.isCoParent)
    //{
    //    var parentGroup = node.containingGroup;
    //    if (parentGroup !== null)
    //    {
    //        parentGroup.invalidateLayout();
    //        if (parentGroup.layout)
    //        {
    //            parentGroup.layout.doLayout(parentGroup); // Manually trigger the layout
    //        }
    //    }
    //}

    //myDiagram.layout.invalidateLayout();
    myDiagram.layoutDiagram(true);

    ModelChanged(true);
}

//----------------------------------------------------------------------------------------------------------------------
// ParentGroup Routines
//----------------------------------------------------------------------------------------------------------------------
function MakeParentGroup(node) {
    if (node.data.node_type == "Detail" || node.data.node_type == 'Vacant' || node.data.node_type == 'Team') {
        if (node !== null) {
            var groupKey = 0;

            // get the breakdown for font, colour, bg_colour, underlined and alignment
            var TeamAttr = deepCopy(MGLB.line_attr);
            var attr = GetLineAttrs(TeamAttr);

            myDiagram.startTransaction("make parent group");
            var groupNodeData = {
                isGroup: true,
                parent: node.data.parent,
                line1: MGLB.parent_group_text,
                line2: "",
                line3: "",
                line4: "",
                line5: "",
                line6: "",
                ds_line1: MGLB.parent_group_text,
                ds_line2: "",
                ds_line3: "",
                ds_line4: "",
                ds_line5: "",
                ds_line6: "",
                line_attr: TeamAttr,
                font1: attr.font[1],
                isUnderline1: attr.isUnderline[1],
                fontColour1: attr.colour[1],
                fontBgColour1: attr.bg_colour[1],
                alignment1: attr.align[1],
                item_ref: "",
                isAssistant: node.data.isAssistant,
                isCoParent: false,
                node_fg: MGLB.node_fg,
                node_bg: MGLB.node_bg,
                node_border_fg: MGLB.node_border_fg,
                node_text_bg: MGLB.node_text_bg,
                node_text_bg_block: MGLB.node_text_bg_block,
                node_icon_fg: MGLB.node_icon_fg,
                node_icon_hover: MGLB.node_icon_hover,
                node_corners: "",
                visible: true,
                node_width: parseInt(MGLB.node_width),
                node_height: parseInt(MGLB.node_height),
                photoshow: false,
                image_height: 0,
                isTreeExpanded: true,
                node_type: "ParentGroup",
                node_tt_fg: "",
                node_tt_bg: "",
                node_tt_border: "",
                tooltip: "",
                label_text: "",
                label_icon: "",
                private_label: false,
                individualphotoshow: false,
                existnode: 1,
                alignment: go.Spot.Left,
                margin: new go.Margin(0, 0, 0, 8),
                category: "ParentGroup",
                sequence: node.data.sequence,
                location: node.data.location,
                isNote: false,
                showShadow: MGLB.showShadow,
                shadowColour: MGLB.shadowColour,
                linkColour: MGLB.linkColour,
                linkHover: MGLB.linkHover,
                linkWidth: MGLB.linkWidth,
                linkType: MGLB.linkType,
                linkTooltipForeground: MGLB.linkTooltipForeground,
                linkTooltipBackground: MGLB.linkTooltipBackground,
                linkTooltipBorder: MGLB.linkTooltipBorder,
                linkTooltip: "",
                nodes_across: 0,
                columns: 1
            };

            myDiagram.model.addNodeData(groupNodeData);
            var newGroup = myDiagram.findNodeForData(groupNodeData);
            groupKey = newGroup.data.key;
            var NextSeq = ResequenceChildrenForGroup(newGroup);

            SetProperty(node.data, "group", groupKey);
            SetProperty(node.data, "parent", groupKey);

            SetProperty(node.data, "isCoParent", true);
            SetProperty(node.data, "isAssistant", node.data.isAssistant);
            SetProperty(node.data, "sequence", NextSeq);
            // This section add for setting line data
            if (node.data.node_type != 'Vacant' && node.data.node_type != 'Team') {
                prepareModelDataItemForType('CoParent', node, MGLB.node_height);
            }

            myDiagram.layout.invalidateLayout();
            myDiagram.commitTransaction("make parent group");

            myDiagram.startTransaction("update related nodes");
            var nodesOut = node.findNodesOutOf();
            if (nodesOut.count > 0) {
                while (nodesOut.next()) {
                    var nodeOut = nodesOut.value;
                    SetProperty(nodeOut.data, "parent", groupKey);
                }
            }

            myDiagram.commitTransaction("update related nodes");

            myDiagram.startTransaction("add link");
            var linksOut = node.findLinksOutOf();
            if (linksOut.count > 0) {
                while (linksOut.next()) {
                    var linkOut = linksOut.value;

                    addModelLink(groupKey, linkOut.data.to);
                    //myDiagram.model.addLinkData({ from: groupKey, to: linkOut.data.to });
                }
                myDiagram.removeParts(node.findLinksOutOf());
            }

            var linksIn = node.findLinksInto();
            if (linksIn.count > 0) {
                while (linksIn.next()) {
                    var linkIn = linksIn.value;

                    addModelLink(linkIn.data.from, groupKey);
                    //myDiagram.model.addLinkData({ from: linkIn.data.from, to: groupKey });
                }
                myDiagram.removeParts(node.findLinksInto());
            }

            myDiagram.commitTransaction("add link");
            groupKey = null;

            ZoomToFit();
            ModelChanged(true);
        }
    }
    return false;
}
function MakeNewParent(node) {
    if (node != null) {
        var parentGroupKey = node.data.parent;
        var parentNode = myDiagram.findNodeForKey(parentGroupKey);
        var nodesIn = parentNode.findNodesInto();
        myDiagram.startTransaction("make new parent");
        myDiagram.removeParts(parentNode.findLinksInto());

        // Set the detail node as a parent node
        if (nodesIn.count == 0) {
            SetProperty(node.data, "parent", undefined);
            SetProperty(node.data, "sequence", 0);
        } else {
            var itParent = myDiagram.findNodeForKey(nodesIn.first().data.key);
            SetProperty(node.data, "parent", itParent.data.key);
            SetProperty(node.data, "sequence", parentNode.data.sequence);
        }
        SetProperty(node.data, "group", undefined);
        SetProperty(node.data, "isCoParent", false);
        SetProperty(node.data, "isAssistant", false);
        SetProperty(node.data, "isTreeExpanded", true);

        // This section add for setting line data
        if (node.data.node_type != 'Vacant' && node.data.node_type != 'Team') {
            prepareModelDataItemForType('Detail', node, MGLB.node_height);
        }
        addModelLink(node.data.key, parentNode.data.key);
        if (nodesIn.count == 1) {
            addModelLink(nodesIn.first().data.key, node.data.key);
        }

        myDiagram.commitTransaction("make new parent");

        // Set parent node as a child of a detail node
        myDiagram.startTransaction("make new parent2");
        if (parentNode != null) {
            SetProperty(parentNode.data, "parent", node.data.key);
            SetProperty(parentNode.data, "sequence", '1');
            SetProperty(parentNode.data, "columns", parentNode.memberParts.count);

            resetSequenceSubGraph(parentNode);
        }

        myDiagram.commitTransaction("make new parent2");
        ModelChanged(true);
    }

}
function MakeChild(node) {
    if (node != null) {
        var nextSeq = 0;
        var groupKey = node.data.parent;

        myDiagram.startTransaction("make child");
        var parentNode = myDiagram.findNodeForKey(groupKey);
        if (parentNode != null) {

            nextSeq = ResequenceChildren(parentNode);
            SetProperty(node.data, "group", undefined);
            SetProperty(node.data, "parent", parentNode.data.key);
            SetProperty(node.data, "isCoParent", false);
            SetProperty(node.data, "isAssistant", false);
            SetProperty(node.data, "sequence", nextSeq);

            // This section add for setting line data
            if (node.data.node_type != 'Vacant' && node.data.node_type != 'Team') {
                prepareModelDataItemForType('Detail', node, MGLB.node_height);
            }

            addModelLink(parentNode.data.key, node.data.key);

            SetProperty(parentNode.data, "columns", parentNode.memberParts.count);
            SetProperty(parentNode.data, "isTreeExpanded", true);
            resetSequenceSubGraph(parentNode);

            myDiagram.commitTransaction("make child");
            ModelChanged(true);
        }

    }

}
function MoveToParentGroup(node) {
    if (node != null) {

        if (node.findTreeChildrenNodes().count == 1) {
            var childNode = node.findTreeChildrenNodes().first();

            if (childNode.data.node_type = 'ParentGroup') {
                var nextSeq = 0;

                var nodesIn = node.findNodesInto();
                myDiagram.removeParts(node.findLinksInto());
                myDiagram.removeParts(node.findLinksOutOf());

                myDiagram.startTransaction("move to parent group");
                var parentNode = myDiagram.findNodeForKey(childNode.data.key);
                if (parentNode != null) {
                    nextSeq = ResequenceChildrenForGroup(parentNode);
                    SetProperty(node.data, "group", parentNode.data.key);
                    SetProperty(node.data, "parent", parentNode.data.key);
                    SetProperty(node.data, "isCoParent", true);
                    SetProperty(node.data, "isAssistant", false);
                    SetProperty(node.data, "sequence", nextSeq);

                    if (nodesIn.count == 0) {
                        SetProperty(parentNode.data, "group", undefined);
                        SetProperty(parentNode.data, "parent", undefined);
                    } else {
                        SetProperty(parentNode.data, "group", undefined);
                        SetProperty(parentNode.data, "parent", nodesIn.first().data.key);
                    }

                    // This section add for setting line data
                    if (node.data.node_type != 'Vacant' && node.data.node_type != 'Team') {
                        prepareModelDataItemForType('CoParent', node, MGLB.node_height);
                    }


                    if (nodesIn.count == 1) {
                        addModelLink(nodesIn.first().data.key, parentNode.data.key);
                        //myDiagram.model.addLinkData({ from: nodesIn.first().data.key, to: parentNode.data.key });
                    }

                    //var colNumber = getColumnNumber(parentNode.memberParts.count);
                    //SetProperty(parentNode.data, "columns", colNumber);
                    SetProperty(parentNode.data, "columns", parentNode.memberParts.count);

                    myDiagram.commitTransaction("move to parent group");
                    ModelChanged(true);
                }
            }
        }

    }

}
function prepareModelDataItemForType(type, node, height) {

    var OCAData = GetModelDataItemForType(type, node.data.item_ref, MGLB.node_height);

    //CopyDefaultAttributes(node);

    var attr = GetLineAttrs(MGLB.line_attr);

    SetProperty(node.data, "line_attr", MGLB.line_attr)

    for (let i = 1; i <= 6; i++) {
        SetProperty(node.data, `line${i}`, OCAData[`line${i}`]);
        SetProperty(node.data, `ds_line${i}`, OCAData[`line${i}`]);
        SetProperty(node.data, `font${i}`, attr.font[i]);
        SetProperty(node.data, `isUnderline${i}`, attr.isUnderline[i]);
        SetProperty(node.data, `fontColour${i}`, attr.colour[i]);
        SetProperty(node.data, `fontBgColour${i}`, attr.bg_colour[i]);
        SetProperty(node.data, `alignment${i}`, attr.align[i]);
    }

    SetProperty(node.data, "node_bg", MGLB.node_bg)
    SetProperty(node.data, "node_fg", MGLB.node_fg)
    SetProperty(node.data, "node_height", parseInt(MGLB.node_height))
    SetProperty(node.data, "node_width", parseInt(MGLB.node_width))
    SetProperty(node.data, "node_corners", MGLB.node_corners)
    SetProperty(node.data, "node_border_fg", MGLB.node_border_fg)
    SetProperty(node.data, "node_text_bg", MGLB.node_text_bg);
    SetProperty(node.data, "node_text_bg_block", MGLB.node_text_bg_block);
    SetProperty(node.data, "node_icon_fg", MGLB.node_icon_fg)
    SetProperty(node.data, "node_icon_hover", MGLB.node_icon_hover)
    SetProperty(node.data, "node_tt_bg", MGLB.node_tt_bg)
    SetProperty(node.data, "node_tt_border", MGLB.node_tt_border)
    SetProperty(node.data, "node_tt_fg", MGLB.node_tt_fg)

    SetProperty(node.data, "show_detail", OCAData.show_detail);

    SetProperty(node.data, "picture_width", parseInt(OCAData.picture_width));
    SetProperty(node.data, "node_table_width",
        CalcNodeTableWidth('Detail', MGLB.show_photos, parseInt(MGLB.node_width), parseInt(OCAData.picture_width)));
    SetProperty(node.data, "source", OCAData.node_picture);
}
function deleteNode(delNode) {
    if (delNode !== null) {
        var ChildCount = 0;
        var ParentNode = null;

        if (delNode.data.isCoParent == true) {

            ParentNode = myDiagram.findNodeForKey(delNode.data.parent);
            ChildCount = getSubNodes(ParentNode).count;
            if (ChildCount == 1) {
                return;
            }
        } else {
            ChildCount = delNode.findTreeChildrenNodes().count;
            ParentNode = delNode.findTreeParentNode();
        }

        if (delNode.data.isGroup == true && delNode.data.node_type == 'ParentGroup') {
            myDiagram.startTransaction("parent node remove");
            var subNodes = [];
            // Find out sub nodes only 
            for (var mit = delNode.memberParts; mit.next();) {
                var part = mit.value; // part is now a Part within the Group
                if (part instanceof go.Node) {
                    subNodes.push(part);
                }
            }

            // if empty parent groups found delete it
            if (subNodes.count == 0 && ChildCount == 0) {
                myDiagram.model.removeNodeData(delNode.data);
                myDiagram.removeParts(delNode.findLinksInto());
            }

            // if multiple co-parent exists then return it 
            if (subNodes.length > 1) {
                return;
            }
            // Only one subnodes should exists
            var subNode = subNodes[0].part;
            if (subNode != null) {
                var NextSeq = 0;
                var parentKey = 0;
                if (ParentNode != null) {
                    //NextSeq = ResequenceChildren(ParentNode);
                    NextSeq = delNode.data.sequence;
                    parentKey = ParentNode.data.key;
                } else {
                    NextSeq = delNode.data.sequence;
                    parentKey = delNode.data.parent;
                }

                // This section add for setting line data
                if (subNode.data.node_type != 'Vacant' && subNode.data.node_type != 'Team') {
                    prepareModelDataItemForType('Detail', subNode, MGLB.node_height);
                }

                SetProperty(subNode.data, "parent", parentKey);
                SetProperty(subNode.data, "sequence", NextSeq);
                SetProperty(subNode.data, "group", '');
                if (subNode.data.isCoParent == true) {
                    SetProperty(subNode.data, "isCoParent", false);
                    SetProperty(subNode.data, "isAssistant", delNode.data.isAssistant);
                }
                NextSeq = 0;
                NextSeq = ResequenceChildren(subNode);

                // attach any child nodes to the node's parent
                var child = delNode.findTreeChildrenNodes();
                while (child.next()) {
                    SetProperty(child.value.data, "parent", subNode.data.key);
                    SetProperty(child.value.data, "sequence", NextSeq);
                    NextSeq++;
                }
                var linksIn = delNode.findLinksInto();
                if (linksIn.count > 0) {
                    while (linksIn.next()) {
                        var linkIn = linksIn.value;
                        addModelLink(linkIn.data.from, subNode.data.key);
                    }
                }

                var linksOut = delNode.findLinksOutOf();
                if (linksOut.count > 0) {
                    while (linksOut.next()) {
                        var linkOut = linksOut.value;
                        addModelLink(subNode.data.key, linkOut.data.to);
                        //myDiagram.model.addLinkData({ from: subNode.data.key, to: linkOut.data.to });
                    }

                }

                myDiagram.removeParts(delNode.findLinksInto());
                myDiagram.removeParts(delNode.findLinksOutOf());
                myDiagram.model.removeNodeData(delNode.data);

            }
            myDiagram.commitTransaction("parent node remove");
        } else {
            // may not delete root node of it has more than one child
            if (ParentNode == null && ChildCount > 1) return;

            // if detail node and we can delete it, then add the node back to the AEA
            if (delNode.data.node_type == "Detail") {
                GetUnallocatedDetail(delNode.data.item_ref);
                BuildAEA();
            }

            if (ParentNode == null && ChildCount < 2) {
                myDiagram.startTransaction("node remove");
                if (delNode.data.isCoParent) {
                    var parentGroupNode = myDiagram.findNodeForKey(delNode.data.parent);
                    if (parentGroupNode.memberParts.count == 1) {
                        return;
                    } else {
                        myDiagram.model.removeNodeData(delNode.data);
                    }
                } else {
                    // deleting the root node
                    var childNode = delNode.findTreeChildrenNodes().first();
                    if (childNode) {
                        myDiagram.removeParts(childNode.findLinksInto(), true);
                        SetProperty(childNode.data, "parent", 0);
                    }

                    // Remove the root node from the diagram
                    myDiagram.model.removeNodeData(delNode.data);
                }

                myDiagram.commitTransaction("node remove");

            } else {
                if (ParentNode != null) {
                    if (delNode.data.isCoParent == false) {
                        var NextSeq = 0;
                        NextSeq = ResequenceChildren(ParentNode);
                        // attach any child nodes to the node's parent
                        var child = delNode.findTreeChildrenNodes();
                        while (child.next()) {
                            SetProperty(child.value.data, "parent", ParentNode.data.key);
                            SetProperty(child.value.data, "sequence", NextSeq);
                            myDiagram.removeParts(child.value.findLinksInto());

                            addModelLink(ParentNode.data.key, child.value.data.key);

                            NextSeq++;
                        }
                        myDiagram.removeParts(delNode.findLinksInto());
                        myDiagram.model.removeNodeData(delNode.data);
                        resetSequenceTreeChildren(ParentNode);
                    } else {
                        myDiagram.startTransaction("node remove");
                        myDiagram.removeParts(delNode.findLinksInto());
                        myDiagram.model.removeNodeData(delNode.data);
                        //var colNumber = getColumnNumber(ParentNode.memberParts.count);
                        //SetProperty(ParentNode.data, "columns", colNumber);
                        SetProperty(ParentNode.data, "columns", ParentNode.memberParts.count);
                        myDiagram.commitTransaction("node remove");
                        resetSequenceSubGraph(ParentNode);
                    }
                }
            }
        }

        ModelChanged(true)
    }
}
function MoveUpToParentGroup(selnode)
{
    var diagram = myDiagram;
    var grp = diagram.selection.first().findTreeParentNode()
    var groupKey = grp.data.key;
    
    // if  node type is team, parent group or note then it will not be possible to drop
    if (selnode == null) {
        return false;
    }

    var previousGroup = myDiagram.findNodeForKey(selnode.data.group);
    if (previousGroup != null) {
        var countMem = getSubNodes(previousGroup);
        if (selnode.data.isCoParent == true && countMem.count < 2) {
            return;
        }
    }

    /* if (selnode.data.isCoParent == true) {
         return;
     }*/

    //if (selnode.data.node_type == 'ParentGroup') {
    //    e.diagram.currentTool.doCancel();
    //    return false;
    //}

    // If parent node wants to drag and drop into parent Group, it will not possible
    if (grp.data.node_type == 'ParentGroup' && grp.data.parent == selnode.data.key) {
        return;
    }

    if (selnode.data.node_type == 'Vacant' && selnode.data.isCoParent == true) {
        return;
    }
    // if (selnode.data.parent == grp.data.key && selnode.data.node_type == 'Team') {
    //     return;
    // }
    if (selnode.data.isCoParent == true && selnode.data.parent == grp.data.key) {
        return;
    }

    if (mayWorkFor(selnode, grp)) {

        // if  the parent group and selected node is in the same parent then it will drop inside the parent group
        if (((selnode.data.dragtype == 'aea' && selnode.data.parent == grp.data.key) ||
                (selnode.data.dragtype == 'oca' && selnode.data.parent == grp.data.key) ||
                (selnode.data.dragtype == 'oca' && selnode.data.isCoParent))) {
            var parentNode = myDiagram.findNodeForKey(groupKey);
            if (parentNode != null) {

                prepareCoParentNode(parentNode, selnode);
                SetProperty(selnode.data, "dragtype", 'oca');
            }
            // For changing Group layout
            SetProperty(grp.data, "columns", grp.memberParts.count);
            SetProperty(grp.data, "isTreeExpanded", true);
            resetSequenceTreeChildren(parentNode);
            resetSequenceSubGraph(parentNode);

            // Changing Previous group layout
            if (selnode.data.isCoParent == true && previousGroup != null) {
                SetProperty(previousGroup.data, "columns", previousGroup.memberParts.count);
            }

            var ok = (grp !== null ?
                grp.addMembers(grp.diagram.selection, true) :
                e.diagram.commandHandler.addTopLevelParts(e.diagram.selection, true));

            if (!ok) return;
            ModelChanged(true);
        } else {
            // if  selected node is granchild/greatgrand child then it will drop outside of the parent group as a child
            myDiagram.startTransaction("add node as a child");
            myDiagram.removeParts(selnode.findLinksInto());
            var parentNode = myDiagram.findNodeForKey(groupKey);
            if (parentNode != null) {
                prepareChildNode(parentNode, selnode);
                SetProperty(parentNode.data, "isTreeExpanded", true);

                addModelLink(parentNode.data.key, selnode.data.key);
                //myDiagram.model.addLinkData({ from: parentNode.data.key, to: selnode.data.key });
            }

            myDiagram.commitTransaction("add node as a child");

            ModelChanged(true);
        }
    }
}
function mayWorkFor(node1, node2) {
    iawVerbose("mayWorkFor()");

    if (!(node1 instanceof go.Node)) return false; // must be a Node
    if (node1 === node2) return false; // cannot work for yourself
    if (node2.isInTreeOf(node1)) return false; // cannot work for someone who works for you
    return true;
}
// This method make a node as co child
function prepareCoParentNode(parentNode, selnode) {
    var nextSeq = 0;
    if (parentNode != null) {
        nextSeq = ResequenceChildrenForGroup(parentNode);

        // This section add for setting line data
        if (selnode.data.node_type != 'Vacant' && selnode.data.node_type != 'Team') {
            prepareModelDataItemForType('CoParent', selnode, MGLB.node_height);
        }
        SetProperty(selnode.data, "group", parentNode.data.key);
        SetProperty(selnode.data, "parent", parentNode.data.key);

        SetProperty(selnode.data, "isCoParent", true);
        SetProperty(selnode.data, "isAssistant", false);
        SetProperty(selnode.data, "sequence", nextSeq);

        var nodesOut = selnode.findNodesOutOf();
        if (nodesOut.count > 0) {
            while (nodesOut.next()) {
                nextSeq = ResequenceChildren(parentNode);
                var nodeOut = nodesOut.value;
                SetProperty(nodeOut.data, "parent", parentNode.data.key);
                SetProperty(nodeOut.data, "sequence", nextSeq);
            }
        }

        var linksOut = selnode.findLinksOutOf();
        if (linksOut.count > 0) {
            while (linksOut.next()) {
                var linkOut = linksOut.value;

                addModelLink(parentNode.data.key, linkOut.data.to);
                //myDiagram.model.addLinkData({ from: parentNode.data.key, to: linkOut.data.to });
            }
        }
        myDiagram.removeParts(selnode.findLinksInto());
        myDiagram.removeParts(selnode.findLinksOutOf());

    }
}

// This method make a node as child
function prepareChildNode(parentNode, selnode) {
    var NextSeq = 0;
    if (parentNode != null) {
        NextSeq = ResequenceChildren(parentNode);

        SetProperty(selnode.data, "group", undefined);
        SetProperty(selnode.data, "parent", parentNode.data.key);
        SetProperty(selnode.data, "isCoParent", false);
        SetProperty(selnode.data, "isAssistant", false);
        SetProperty(selnode.data, "sequence", NextSeq);

    }
}
//----------------------------------------------------------------------------------------------------------------------
// Node Expand /  Routines
//----------------------------------------------------------------------------------------------------------------------
function collapseFrom(node) {
    if (node !== null) {
        node.findTreeChildrenNodes().each(child => {
            collapseFrom(child);
        });

        node.collapseTree();
        SetProperty(node.data, "isTreeExpanded", false);
    }
}
function expandFrom(node) {
    if (node !== null) {
        SetProperty(node.data, "isTreeExpanded", true);
        node.findTreeChildrenNodes().each(expandFrom);
    }
}
function hasCollapsedDescendants(node) {
    if (node == null) return false;  // Early return for null node

    if (node.findTreeChildrenNodes().count === 0) return false;  // Return false if no children

    let foundCollapsed = false;

    node.findTreeParts().each(part => {
        if (part instanceof go.Node || part instanceof go.Group) {
            if (part.findTreeChildrenNodes().count > 0 && !part.isTreeExpanded)
            {
                foundCollapsed = true;
                return false;  // Stop iteration early since we've found a collapsed node
            }
        }
    });

    return foundCollapsed;
}

//----------------------------------------------------------------------------------------------------------------------
// Misc Routines
//----------------------------------------------------------------------------------------------------------------------
function SetProperty(data, path, value) {
    myDiagram.model.setDataProperty(data, path, value);
}
function ctrlsDisable(ctrls) {
    ctrls.forEach((ctrl) => {
        $ID(ctrl).setAttribute("disabled", "disabled");
        $ID(ctrl).classList.add("stopClicks");
    });
}
function ctrlsEnable(ctrls) {
    ctrls.forEach((ctrl) => {
        $ID(ctrl).removeAttribute("disabled");
        $ID(ctrl).classList.remove("stopClicks");
    });
}
function ChartOrientation(...values) {
    return values.includes(MGLB.chartDirection);
}

// function to set the value of a dropdown
function setDropDownListSelectedValue(ddl, value) {
    for (var i = 0; i < ddl.options.length; i++) {
        if (ddl.options[i].value == value) {
            ddl.selectedIndex = i;
            break;
        }
    }
}
function setRadioButtonListSelectedValue(rbl, value) {
    var inputs = rbl.getElementsByTagName('input');
    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].type === 'radio' && inputs[i].value == value) {
            inputs[i].checked = true; // Set the checked property to true for the matching radio button
            break;
        }
    }
}

function SetChartBackground() {
    let Chart = $ID('myDiagramDiv').style;
    Chart.backgroundImage = "";
    Chart.background = "";
    Chart.backgroundRepeat = 'repeat';
    Chart.backgroundSize = 'auto';
    switch (MGLB.backgroundType.toLowerCase()) {
        case "gradient":
            Chart.background = MGLB.background;
            break;
        case "image":
            // now lets process the image types
            Chart.backgroundImage = "url('../backgrounds/" + MGLB.background + "')";
            switch (MGLB.backgroundRepeat) {
                case "01": // no repeat
                    Chart.backgroundRepeat = "no-repeat";
                    break;
                case "02": // fit to screen
                    Chart.backgroundRepeat = "no-repeat";
                    Chart.backgroundSize = 'cover';
                    break;
                case "03": // repeat
                    break;
                case "04": // repeat across
                    Chart.backgroundRepeat = "repeat-x";
                    break;
                case "05": // repeat down
                    Chart.backgroundRepeat = "repeat-y";
                    break;
                case "06": // fit across
                    Chart.backgroundRepeat = "no-repeat";
                    Chart.backgroundSize = '100% auto';
                    break;
            }
    }
    Chart.backgroundColor = MGLB.backgroundContent
}
function GetLinkLineType(data) {
    var lineType = null;
    if (data) {
        switch (data.linkType) {
            case "solid":
                break;
            case "dotted":
                lineType = [3, 4];
                break;
            case "dashes":
                lineType = [15, 5];
                break;
        }
    }
    return lineType;
}

// Function to toggle the visibility of buttons
function toggleButtons(button_position) {
    myDiagram.startTransaction("ApplyNodeSetting");
    
    MGLB.button_position = button_position;
    myDiagram.commitTransaction('ApplyNodeSetting');
    myDiagram.rebuildParts();
    myDiagram.requestUpdate();

    ModelChanged(true);
}

const svgCache = new Map();
function loadSVGIcon(iconPath, color, width = 250, height = 250) {
    const cacheKey = `${iconPath}_${color}`;

    // Remove previous cache entries for the same iconPath if they exist
    for (let key of svgCache.keys()) {
        if (key.startsWith(iconPath) && key !== cacheKey) {
            svgCache.delete(key);
        }
    }

    if (svgCache.has(cacheKey)) {
        return Promise.resolve(svgCache.get(cacheKey));
    }

    return fetch(iconPath)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.text();
        })
        .then(svgText => {
            // Modify the first fill attribute in the SVG text
            let coloredSvgText = svgText.replace(/<path([^>]*)>/g, (match, group1) => {
                if (/fill="[^"]*"/.test(group1)) {
                    return `<path${group1.replace(/fill="[^"]*"/, `fill="${color}"`)}>`;
                } else {
                    return `<path fill="${color}"${group1}>`;
                }
            });

            // Add or update the width and height attributes in the SVG text
            coloredSvgText = coloredSvgText.replace(/<svg[^>]*>/, (match) => {
                return match
                    .replace(/width="[^"]*"/, '')
                    .replace(/height="[^"]*"/, '')
                    .replace(/>/, ` width="${width}" height="${height}">`);
            });

            const base64Svg = btoa(coloredSvgText);
            const dataUrl = 'data:image/svg+xml;base64,' + base64Svg;

            // Cache the result
            svgCache.set(cacheKey, dataUrl);

            return dataUrl;
        })
        .catch(error => {
            iawError('There has been a problem with your fetch operation:', error);
        });
}
function rgbToHex(rgb) {
    if (rgb.trim() == "")    return "#000000";      // empty string, return black
    if (rgb.startsWith("#")) return rgb;            // already in hex format
    const result = rgb.match(/\d+/g).map(Number);
    return `#${((1 << 24) + (result[0] << 16) + (result[1] << 8) + result[2]).toString(16).slice(1)}`;
}
