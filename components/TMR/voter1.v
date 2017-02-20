module voter1(out, a, b, c)/* synthesis syn_sharing = "off" */; // voter
	output  out;
	input a, b, c;
	wire eq;
	assign eq=(a== b);
	assign out = eq? a :c;
endmodule