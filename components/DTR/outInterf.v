//Output Block (if needed at all)
module outBlock (clk, co, outNew, 
					save,rollBack,subst, fail);
	input  clk; // clock signal
	input  co;//original primary input of the circuit
	output[2:0] outNew; //new input for the original circuit
	input  save; 
	input  rollBack; 
	input  subst; 
	output fail;
	// Registers
	reg o1/*synthesis syn_keep = 1*/;
	reg o2/*synthesis syn_keep = 1*/;
	reg o3/*synthesis syn_keep = 1*/;
	reg p1/*synthesis syn_keep = 1*/;
	reg p2/*synthesis syn_keep = 1*/;
	//Assignments
	assign fail= (o1!=o2); // error detection
	
	wire sub;
	assign sub= (save)? co:o1;
	
	wire coA/*synthesis syn_keep = 1*/;
	wire coB/*synthesis syn_keep = 1*/;
	assign coA= co;
	assign coB= co;
	
	
	wire cA/*synthesis syn_keep = 1*/;
	wire cB/*synthesis syn_keep = 1*/;
	wire cC/*synthesis syn_keep = 1*/;
	
	wire rollA/*synthesis syn_keep = 1*/;
	wire rollB/*synthesis syn_keep = 1*/;
	wire rollC/*synthesis syn_keep = 1*/;
	
	wire subsA/*synthesis syn_keep = 1*/;
	wire subsB/*synthesis syn_keep = 1*/;
	wire subsC/*synthesis syn_keep = 1*/;
	
	assign rollA= rollBack;
	assign rollB= rollBack;
	assign rollC= rollBack;
	
	assign subsA= subst;
	assign subsB= subst;
	assign subsC= subst;
	
	assign cA= rollA&&subsA;
	assign cB= rollB&&subsB;
	assign cC= rollC&&subsC;
	
	//Definition of Output Value
	assign outNew[0]= (cA)?sub:p2;
	assign outNew[1]= (cB)?sub:o2;
	assign outNew[2]= (cC)?sub:o3;
	//Process					
	always @(posedge clk)
	begin
		o1<=coA;
		o2<=o1;
		o3<=o2;
		
		p1<=coB;
		p2<=p1;
	end
endmodule