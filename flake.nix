{
  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixpkgs-unstable";
  };

  outputs = {nixpkgs}: let
    pkgs = nixpkgs.legacyPackages."x86_64-linux";
    lib = nixpkgs.lib;
  in {
    devShells."x86_64-linux".default = pkgs.mkShell {
      packages = with pkgs; [
        dotnet-sdk_8
        wmctrl
      ];

      LD_LIBRARY_PATH = with pkgs;
        lib.makeLibraryPath [
          stdenv.cc.cc
          fontconfig
          xorg.libX11
          xorg.libICE
          xorg.libSM
        ];
      NIX_LD = "${pkgs.stdenv.cc}/bin/ld";
    };
  };
}
