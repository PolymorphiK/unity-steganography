<?php
    header('content-type: application/json');
    $result->code = 200;
    $result->error = "";
    $result->message = "";
   
    switch($_SERVER['REQUEST_METHOD']) {
        case 'GET': {
            $list->images=glob("public/*.{png,bmp,jpg,jpeg}", GLOB_BRACE);
            $list->url='http://athena.ecs.csus.edu/~pachecok/153';
            echo json_encode($list); return;
        } break;
        case 'POST': {
            try {
                $contents = file_get_contents('php://input');
                $contents = utf8_encode($contents);
                $data= json_decode($contents, true);
                
                if(array_key_exists('image', $data) == false) {
                    $result->error = 'Requires \'image\' property.';
                    $result->code = 400;

                    echo json_encode($result); return;
                }

                if(array_key_exists('type', $data) == false) {
                    $result->error = 'Requires \'type\' property.';
                    $result->code = 400;
                    echo json_encode($result); return;
                }
                
                $raw_image = base64_decode($data['image']);
                $image = 'public/' . hash('md5', $data['image']) . '.' . $data['type'];
                file_put_contents($image, $raw_image);
                
                $result->message ="Image saved.";
                $result->payload = $image;
                echo json_encode($result); return;
            } catch(Exception $e) {
                echo $e->getMessage(); return;
            }
        } break;
    } 
?>
