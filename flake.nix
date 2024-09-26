{
  description = "A very basic flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixpkgs-unstable";
  };

  outputs = {
    self,
    nixpkgs,
  }: let
    pkgs = nixpkgs.legacyPackages."x86_64-linux";
    lib = nixpkgs.lib;
  in {
    devShells."x86_64-linux".default = pkgs.mkShell {
      buildInputs = with pkgs; [
        dotnet-sdk_8
        dotnet-runtime_8
        avalonia
        dotnetPackages.Nuget
        mono
        skia

        xorg.libX11
        xorg.libX11.dev
        xorg.libICE
        xorg.libSM
        xorg.libXinerama
        fontconfig
        libGL
        libxkbcommon
        glib
        freetype
      ];

      LD_LIBRARY_PATH = with pkgs; lib.makeLibraryPath [
        stdenv.cc.cc
        skia
        fontconfig
        fontconfig
        xorg.libX11
        xorg.libICE
        xorg.libSM
        dotnet-sdk_8
      ];
      NIX_LD = "${pkgs.stdenv.cc}/bin/ld";
    };
  };
}
