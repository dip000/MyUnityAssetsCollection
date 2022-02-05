

        function DictionaryToVector2(dictionary){
			let arrayX = [];
			let arrayY = [];
			
            for(let i=0; i<Object.keys(dictionary).length; i++){
                arrayX[i] = dictionary[ Object.keys(dictionary)[i] ].x;
                arrayY[i] = dictionary[ Object.keys(dictionary)[i] ].y;
            }
			
            console.log("VECTOR TO DOUBLE ARRAY: ");
            console.log(arrayX)
            console.log(arrayY)
			
			return new Vector2Array(arrayX, arrayY);
       }


        function LocalizeCoordenates(vector2){
            var minX = 999;
            var minY = 999;

            for(var i=0; i<vector2.x.length; i++){
                if(vector2.x[i] < minX){
                    minX = vector2.x[i];
                }
                if(vector2.y[i] < minY){
                    minY = vector2.y[i];
                }
           }

            for(var i=0; i<vector2.x.length; i++){
                vector2.x[i] -= minX;
                vector2.y[i] -= minY;
            }

            console.log("LOCALIZED TO MIN VALUE: ");
            console.log("minX " + minX + "; minY " + minY);
            console.log(vector2);

			return vector2;
        }
		
		function GlobalizeCoordenates(shape, x, y){			
			let coordenates = new Vector2Array(shape);
			
            for(let i=0; i<coordenates.x.length; i++){
                coordenates.x[i] += x;
                coordenates.y[i] += y;
            }
            //console.log("GLOBALIZED TO TARGET VALUE: ");
            //console.log(coordenates);
			return coordenates;
		}
		
		function RotateCoordenatesByAngle(coordenates, angle){
			//console.log("RESULT angle: " + angle);
			
			if(angle<0){
				angle = -(angle%360)/90;
			}
			else{
				angle = 4-(angle%360)/90;
			}
			
			angle = Math.round(angle);
			
			if(angle == 4) return coordenates;
			
			let rotatedCoordenates = new Vector2Array(coordenates);
			
			//console.log("RESULT times: " + angle);
			for(let i=0; i<angle; i++){
				 rotatedCoordenates = RotateCoordenates90Clockwise(rotatedCoordenates);
			}
			
			return rotatedCoordenates;
		}
		
		
        function RotateCoordenates90Clockwise(vector2){
			
            //Flip axis. This actually mirors coordenates by 45 degrees
            var maxY = 0;
            for(var i=0; i<vector2.x.length; i++){
				var switchReg = vector2.x[i];
                vector2.x[i] = vector2.y[i];
                vector2.y[i] = switchReg;

                if(vector2.y[i] > maxY){
                    maxY =  vector2.y[i];
                }
            }

            //Miror y axis. Both instructions actually rotate the coordenates 90° counteclockwise
            // and that can be seen as switching from row-cols system to x-y cartesian system
            for(var i=0; i<vector2.x.length; i++){
                vector2.y[i] = maxY - vector2.y[i];
            }
           
            //console.log("ROTATED BY 90 CLOCKWISE: ");
            //console.log(vector2);
			
			return vector2;
       }
	   