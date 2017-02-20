



// Central Control FSM- HeartBeat
module ctr2DynT(clk, reset, // standart inputs
					fail, modeS,  // to manipulate Memory blocks
					userMode, // to manipulte the reading of Input buffer
					userFail
					);
	input clk; //global synch. clock
	input reset; //global reset signal
	input fail; // error detection wire
	output modeS;// to manipulate Memory blocks
	input userMode; // to manipulte the reading of Input buffer
	output userFail; 
	
	//Registers
	reg modeReg;
	reg failReg;
	
	assign modeS= modeReg;
	assign userFail=failReg;
	
always @ (posedge clk or posedge reset)
	begin 
		if (reset)
			begin				
				modeReg<=0; 
				failReg<=0;
			end
		else 
			begin: fsm
				modeReg<=userMode; 
				failReg<= fail;
			end
	end
endmodule

//NOTE; such voting will save only agains 1 SEU, synchronization is not established
//TODO: create re-synchronization between counters-> should be efficient
//ctr[3]||reset dropped frequency from 361 to <150Mhz-> so no the best choice

// module ctr2RecTmr(clk, reset, ctr, errFlag); // TMR counter for TTR ctr signals
	// input	clk; //global synch. clock
	// input 	[2:0]reset; //global reset sygnal
	// output	[1:0]ctr; //control outpus for Time Redundancy
	// input	[2:0]errFlag;// Error Flag- indicates that the recovery procedure is in progress and 00 in ctr is required
	
	// wire [5:0]ctrL;
	// // counters instantiation
	// genvar i;
	// generate
		// for (i=0;i<3; i=i+1)
		// begin: ripple
			// ctr2RecT count(clk, reset[i],ctrL[1+2*i:0+2*i], errFlag[i])/*synthesis syn_preserve = 1*/; 
		// end
	// endgenerate
	// // voters instantiation
	// genvar i;
	// generate
	// for (i=0;i<2; i=i+1)
		// begin: ripple
			// voter1 V(ctr[i],ctrL[0+i],ctrL[2+i],ctrL[4+i])/*synthesis syn_preserve = 1*/; 
		// end
	// endgenerate
// endmodule	
	
	