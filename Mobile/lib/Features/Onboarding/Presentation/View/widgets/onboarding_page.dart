import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Shared/onboarding_image.dart';
import 'package:medlink/Core/Shared/onboarding_tile.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Features/Onboarding/Presentation/Models/onboarding_item.dart';
import 'package:smooth_page_indicator/smooth_page_indicator.dart';

/// One onboarding page. The same widget is used for the 3 pages,
/// only the [item] changes.
class OnboardingPage extends StatelessWidget {
  final OnboardingItem item;
  final PageController controller;
  final int count;

  const OnboardingPage({
    super.key,
    required this.item,
    required this.controller,
    required this.count,
  });

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: EdgeInsets.symmetric(horizontal: 16.w),
      child: Column(
        children: [
          OnboardingImage(assetPath: item.image),
          SizedBox(height: 8.w),
          SmoothPageIndicator(
            controller: controller,
            count: count,
            effect: ExpandingDotsEffect(
              dotHeight: 6.w,
              dotWidth: 6.w,
              expansionFactor: 4,
              spacing: 6.w,
              activeDotColor: ColorsManager.primaryColor,
              dotColor: const Color(0xffD3E3F8),
            ),
          ),
          SizedBox(height: 20.w),
          Text(
            item.title,
            textAlign: TextAlign.center,
            style: TextStyles.authTitle.copyWith(fontSize: 24),
          ),
          SizedBox(height: 8.w),
          Text(
            item.description,
            textAlign: TextAlign.center,
            style: TextStyles.fadedText.copyWith(fontSize: 13, height: 1.5),
          ),
          SizedBox(height: 20.w),
          Column(
            spacing: 10.w,
            children: [
              for (final tile in item.tiles)
                OnboardingTile(
                  icon: tile.icon,
                  title: tile.title,
                  subtitle: tile.subtitle,
                  badge: tile.badge,
                  iconColor: tile.iconColor,
                  badgeColor: tile.badgeColor,
                ),
            ],
          ),
          SizedBox(height: 16.w),
        ],
      ),
    );
  }
}
