import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/svg.dart';
import 'package:medlink/Core/Style/text_styles.dart';

class AuthScaffold extends StatelessWidget {
  final logo;
  final String title, titleDescription;
  final bool? isCentered;
  final Widget child;
  const new({
    super.key,
    required this.logo,
    required this.title,
    required this.titleDescription,
    required this.child,
    this.isCentered,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        crossAxisAlignment: .stretch,
        children: [
          Padding(
            padding: EdgeInsets.only(
              left: 16,
              right: 16,
              top: 24.w,
              bottom: 12.w,
            ),
            child: Column(
              children: [
                Column(
                  spacing: 20.w,
                  children: [
                    Column(
                      children: [
                        SizedBox(
                          width: 340.w,
                          height: 50.w,
                          child: Image.asset(logo),
                        ),
                        Text(
                          'Your Health, Our Connection.',
                          style: TextStyles.fadedText,
                        ),
                      ],
                    ),
                    Row(
                      mainAxisAlignment: isCentered == true ? .start : .center,
                      children: [
                        Column(
                          crossAxisAlignment: isCentered == true
                              ? .start
                              : .center,
                          children: [
                            Text(title, style: TextStyles.authTitle),
                            Text(titleDescription, style: TextStyles.fadedText),
                          ],
                        ),
                      ],
                    ),

                    child,
                  ],
                ),
              ],
            ),
          ),
          Expanded(
            child: SvgPicture.asset(
              'assets/svgs/auth_svg.svg',
              width: double.infinity,
            ),
          ),
        ],
      ),
    );
  }
}
