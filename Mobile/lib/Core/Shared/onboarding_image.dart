import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/flutter_svg.dart';

/// Shared hero image for the onboarding pages.
/// Works with both PNG/JPG and SVG assets.
class OnboardingImage extends StatelessWidget {
  final String assetPath;
  final double? height;

  const OnboardingImage({super.key, required this.assetPath, this.height});

  bool get _isSvg => assetPath.toLowerCase().endsWith('.svg');

  @override
  Widget build(BuildContext context) {
    final double h = height ?? 260.w;

    return SizedBox(
      width: double.infinity,
      height: h,
      child: _isSvg
          ? SvgPicture.asset(assetPath, fit: BoxFit.contain)
          : Image.asset(assetPath, fit: BoxFit.contain),
    );
  }
}
