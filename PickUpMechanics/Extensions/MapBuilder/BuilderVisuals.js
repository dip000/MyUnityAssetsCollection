

// LISTENERS //////////////////////////////////////////////////////////////////////

	var previusCoordenates;
	
	function AddHoverListenerToElements(elements){
		for(let i=0; i<elements.length; i++){
			elements[i].addEventListener('mouseenter', function(e) {
								
				currentItemPlacingInfo.positionX = e.target.parentElement.rowIndex;
				currentItemPlacingInfo.positionY = e.target.cellIndex;
				
				positionX.value = currentItemPlacingInfo.positionX;
				positionY.value = currentItemPlacingInfo.positionY;
		
				printHoverVisuals();
				
			}, false);
		}
	}
	

	function AddClickListenerToElement(element){
		element.addEventListener('click', function(e) {
			console.log("Clicked at: " + e.target.parentElement.rowIndex + "," + e.target.cellIndex);
		   let x = e.target.parentElement.rowIndex;
		   let y = e.target.cellIndex;
		   
		   if(x == null || y == null) return;
		   OnGridClick(x, y);
		   
		}, false);     
	}
	
	function AddListenerToScrollWheel(item){
		item.addEventListener("wheel", function(e) {
			e.preventDefault();
			
			let rot = 90;
			if( e.deltaY < 0){
				rot = -90;
			}
			
			console.log("SCROLL EVENT. rotation to: " + rot);
			console.log(currentItemPlacingInfo);
			currentItemPlacingInfo.rotation = (rot+currentItemPlacingInfo.rotation)%360;
			
			rotation.value = currentItemPlacingInfo.rotation;
			
			printHoverVisuals();
		});
	}
	
////////////////////////////////////////////////////////////////////////////////////
	
	
// PRINTERS ////////////////////////////////////////////////////////////////////////
		
	function printHoverVisuals(){		
		//Read all from current placing info
		let shape = listOfShapes[currentItemPlacingInfo.itemType];
		let shapeRotated = RotateCoordenatesByAngle(shape, currentItemPlacingInfo.rotation);
		let coordenates = GlobalizeCoordenates(shapeRotated, currentItemPlacingInfo.positionX, currentItemPlacingInfo.positionY);
		
		coordenates = IgnoreOccupiedCoordenates(coordenates);
		previusCoordenates = IgnoreOccupiedCoordenates(previusCoordenates);
		
		printVisualsOfCoordenates( previusCoordenates , "white" );
		printVisualsOfCoordenates( coordenates, "lightBlue" );
		
		previusCoordenates = coordenates;
	}
	
	
	function printVisualsOfCoordenates(shape, color){
		if(shape==null) return;
		for(var i=0; i<shape.x.length; i++){
			var cell = table.rows[ shape.x[i] ].cells[ shape.y[i] ];
			cell.style.backgroundColor = color;	
		}
	}
	
	
	
	var isPrintAction = true;
	function ToggleDotInCoordenate(x, y){
		var cell = table.rows[x].cells[y];
		var cellColor = cell.style.backgroundColor

		if(cellColor == "red"){
			cell.style.backgroundColor = "white";
			isPrintAction = false;
		}
		else{
			cell.style.backgroundColor = "red";
			isPrintAction = true;
		}
	}

////////////////////////////////////////////////////////////////////////////////////


// TYPES AND CONSTRUCTORS //////////////////////////////////////////////////////////

	function generate_table(x, y, tableID) {
		// get the reference for the body
		var body = document.getElementsByTagName("body")[0];

		// creates a <table> element and a <tbody> element
		var tbl = document.createElement("table");
		var tblBody = document.createElement("tbody");

		// creating all cells
		for (var i = 0; i < x; i++) {
			// creates a table row
			var row = document.createElement("tr");

			for (var j = 0; j < y; j++) {
				// Create a <td> element and a text node, make the text
				// node the contents of the <td>, and put the <td> at
				// the end of the table row
				var cell = document.createElement("td");
				var cellText = document.createTextNode("  ");
				cell.appendChild(cellText);
				row.appendChild(cell);
			}

			// add the row to the end of the table body
			tblBody.appendChild(row);
		}

		// put the <tbody> in the <table> and appends into <body>
		tbl.appendChild(tblBody);
		body.appendChild(tbl);
		tbl.setAttribute("border", "2");

		tbl.id = tableID;
	}
	
////////////////////////////////////////////////////////////////////////////////////