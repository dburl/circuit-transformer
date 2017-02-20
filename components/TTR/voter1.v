// Single Voter Module
module voter1(out, a, b, c); // voter
	output  out;
	input a, b, c;
	wire eq/*synthesis syn_keep = 1*/;
	assign eq=(a== b);
	assign out = eq? a :c;
endmodule